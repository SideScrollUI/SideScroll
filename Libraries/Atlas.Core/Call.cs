using Atlas.Extensions;
using System.Runtime.CompilerServices;

namespace Atlas.Core;

[Unserialized]
public class Call
{
	private const int MaxRequestsPerSecond = 10;

	public string? Name { get; set; }

	public Log Log { get; set; }

	public Call? ParentCall { get; set; }

	[Unserialized]
	public TaskInstance? TaskInstance { get; set; } // Shows the Task Status and let's you stop them

	public override string? ToString() => Name;

	protected Call()
	{
		Log = new();
	}

	public Call(string? name = null)
	{
		Name = name;
		Log = new();
	}

	public Call(Log log)
	{
		Log = log;
	}

	public Call Child([CallerMemberName] string name = "", params Tag[] tags)
	{
		Call call = new()
		{
			Name = name,
			ParentCall = this,
			TaskInstance = TaskInstance,
			Log = Log.Call(name, tags),
		};
		return call;
	}

	public Call DebugLogAll()
	{
		var child = Child("LogDebug");
		child.Log.Settings = child.Log.Settings!.Clone();
		child.Log.Settings.MinLogLevel = LogLevel.Debug;
		child.Log.Settings.DebugPrintLogLevel = LogLevel.Debug;
		return child;
	}

	public CallTimer Timer([CallerMemberName] string? name = null, params Tag[] tags)
	{
		return Timer(LogLevel.Info, name, tags);
	}

	public CallTimer Timer(LogLevel logLevel, [CallerMemberName] string? name = null, params Tag[] tags)
	{
		var call = new CallTimer()
		{
			Name = name,
			ParentCall = this,
		};
		call.TaskInstance = TaskInstance?.AddSubTask(call);
		call.Log = Log.Call(logLevel, name ?? "Timer", tags);

		return call;
	}

	public CallTimer StartTask([CallerMemberName] string? name = null, params Tag[] tags)
	{
		return StartTask(LogLevel.Info, name, tags);
	}

	public CallTimer StartTask(LogLevel logLevel, [CallerMemberName] string? name = null, params Tag[] tags)
	{
		var call = new CallTimer()
		{
			Name = name,
			ParentCall = this,
			IsTask = true,
		};
		call.TaskInstance = TaskInstance?.AddSubTask(call) ?? new TaskInstance(name)
		{
			TaskCount = 1,
		};
		call.Log = Log.Call(logLevel, name ?? "Task", tags);

		return call;
	}

	public CallTimer StartTask(int taskCount, [CallerMemberName] string name = "", params Tag[] tags)
	{
		TaskInstance ??= new TaskInstance();
		if (TaskInstance.TaskCount == 0)
			TaskInstance.TaskCount = 1;

		var allTags = tags.ToList();
		allTags.Add(new Tag("Count", taskCount));

		CallTimer timer = StartTask(name, allTags.ToArray());
		timer.TaskInstance!.TaskCount = taskCount;
		return timer;
	}

	// allows having progress broken down into multiple tasks
	public TaskInstance AddSubTask(string name = "")
	{
		TaskInstance = TaskInstance!.AddSubTask(Child(name));
		return TaskInstance;
	}

	// allows having progress broken down into multiple tasks
	public Call AddSubCall(string name = "")
	{
		return AddSubTask(name).Call;
	}

	private static async Task<T2?> RunFuncAsync<T1, T2>(Call call, Func<Call, T1, Task<T2>> func, T1 item)
	{
		using CallTimer callTimer = call.StartTask(item?.ToString());

		try
		{
			T2 result = await func(callTimer, item);
			if (result == null)
				callTimer.Log.Add("No result");
			return result;
		}
		catch (Exception e)
		{
			callTimer.Log.Add(e);
			return default;
		}
	}

	private static async Task<T3?> RunFuncAsync<T1, T2, T3>(Call call, Func<Call, T1, T2, Task<T3>> func, T1 item, T2 param1)
	{
		using CallTimer callTimer = call.StartTask(item?.ToString());

		try
		{
			T3 result = await func(callTimer, item, param1);
			if (result == null)
				callTimer.Log.Add("No result");
			return result;
		}
		catch (Exception e)
		{
			callTimer.Log.Add(e);
			return default;
		}
	}

	private static async Task<T4?> RunFuncAsync<T1, T2, T3, T4>(Call call, Func<Call, T1, T2, T3, Task<T4>> func, T1 item, T2 param1, T3 param2)
	{
		using CallTimer callTimer = call.StartTask(item?.ToString());

		try
		{
			T4 result = await func(callTimer, item, param1, param2);
			if (result == null)
				callTimer.Log.Add("No result");
			return result;
		}
		catch (Exception e)
		{
			callTimer.Log.Add(e);
			return default;
		}
	}

	public async Task<List<T2>> RunAsync<T1, T2>(Func<Call, T1, Task<T2?>> func, ICollection<T1> items, int maxRequestsPerSecond = MaxRequestsPerSecond)
	{
		return await RunAsync(func.Method.Name.TrimEnd("Async").WordSpaced(), func, items, maxRequestsPerSecond);
	}

	public async Task<List<T3>> RunAsync<T1, T2, T3>(Func<Call, T1, T2, Task<T3?>> func, ICollection<T1> items, T2 param1, int maxRequestsPerSecond = MaxRequestsPerSecond)
	{
		return await RunAsync(func.Method.Name.TrimEnd("Async").WordSpaced(), func, items, param1, maxRequestsPerSecond);
	}

	public async Task<List<T4>> RunAsync<T1, T2, T3, T4>(Func<Call, T1, T2, T3, Task<T4?>> func, ICollection<T1> items, T2 param1, T3 param2, int maxRequestsPerSecond = MaxRequestsPerSecond)
	{
		return await RunAsync(func.Method.Name.TrimEnd("Async").WordSpaced(), func, items, param1, param2, maxRequestsPerSecond);
	}

	// Call func for every item in the list using the specified parameters
	public async Task<List<T2>> RunAsync<T1, T2>(string name, Func<Call, T1, Task<T2?>> func, ICollection<T1> items, int maxRequestsPerSecond = MaxRequestsPerSecond)
	{
		using CallTimer callTimer = StartTask(items.Count, name);

		using var throttler = new SemaphoreSlim(maxRequestsPerSecond);

		var tasks = new List<Task>();
		var results = new List<T2>();
		foreach (T1 item in items)
		{
			await throttler.WaitAsync();
			if (TaskInstance?.CancelToken.IsCancellationRequested == true)
			{
				Log.Add("Cancelled");
				break;
			}
			tasks.Add(Task.Run(async () =>
			{
				try
				{
					T2? result = await RunFuncAsync(callTimer, func, item);
					if (result != null)
					{
						lock (results)
						{
							results.Add(result);
						}
					}
				}
				catch (Exception e)
				{
					Log.Add(e);
				}
				finally
				{
					throttler.Release();
				}
			}));
		}
		await Task.WhenAll(tasks);

		return results;
	}

	public async Task<List<T3>> RunAsync<T1, T2, T3>(string name, Func<Call, T1, T2, Task<T3?>> func, ICollection<T1> items, T2 param1, int maxRequestsPerSecond = MaxRequestsPerSecond)
	{
		using CallTimer callTimer = StartTask(items.Count, name);

		using var throttler = new SemaphoreSlim(maxRequestsPerSecond);

		var tasks = new List<Task>();
		var results = new List<T3>();
		foreach (T1 item in items)
		{
			await throttler.WaitAsync();
			if (TaskInstance?.CancelToken.IsCancellationRequested == true)
			{
				Log.Add("Cancelled");
				break;
			}
			tasks.Add(Task.Run(async () =>
			{
				try
				{
					T3? result = await RunFuncAsync(callTimer, func, item, param1);
					if (result != null)
					{
						lock (results)
						{
							results.Add(result);
						}
					}
				}
				catch (Exception e)
				{
					Log.Add(e);
				}
				finally
				{
					throttler.Release();
				}
			}));
		}
		await Task.WhenAll(tasks);

		return results;
	}

	public async Task<List<T4>> RunAsync<T1, T2, T3, T4>(string name, Func<Call, T1, T2, T3, Task<T4?>> func, ICollection<T1> items, T2 param1, T3 param2, int maxRequestsPerSecond = MaxRequestsPerSecond)
	{
		using CallTimer callTimer = StartTask(items.Count, name);

		using var throttler = new SemaphoreSlim(maxRequestsPerSecond);

		var tasks = new List<Task>();
		var results = new List<T4>();
		foreach (T1 item in items)
		{
			await throttler.WaitAsync();
			if (TaskInstance?.CancelToken.IsCancellationRequested == true)
			{
				Log.Add("Cancelled");
				break;
			}
			tasks.Add(Task.Run(async () =>
			{
				try
				{
					T4? result = await RunFuncAsync(callTimer, func, item, param1, param2);
					if (result != null)
					{
						lock (results)
						{
							results.Add(result);
						}
					}
				}
				catch (Exception e)
				{
					Log.Add(e);
				}
				finally
				{
					throttler.Release();
				}
			}));
		}
		await Task.WhenAll(tasks);

		return results;
	}
}
