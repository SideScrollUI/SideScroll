using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Logs;
using SideScroll.Tasks;
using SideScroll.Utilities;
using System.Runtime.CompilerServices;

namespace SideScroll;

[Unserialized]
public class Call
{
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
		var call = new CallTimer
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
		var call = new CallTimer
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
		{
			TaskInstance.TaskCount = 1;
		}

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

	private static async Task<TResult?> RunFuncAsync<TItem, TResult>(
		Call call,
		Func<Call, TItem, Task<TResult>> func,
		TItem item)
	{
		using CallTimer callTimer = call.StartTask(item?.ToString());

		try
		{
			TResult result = await func(callTimer, item);
			if (result == null)
			{
				callTimer.Log.Add("No result");
			}
			return result;
		}
		catch (Exception e)
		{
			callTimer.Log.Add(e);
			return default;
		}
	}

	private static async Task<TResult?> RunFuncAsync<TItem, TParam1, TResult>(
		Call call,
		Func<Call, TItem, TParam1, Task<TResult>> func,
		TItem item,
		TParam1 param1)
	{
		using CallTimer callTimer = call.StartTask(item?.ToString());

		try
		{
			TResult result = await func(callTimer, item, param1);
			if (result == null)
			{
				callTimer.Log.Add("No result");
			}
			return result;
		}
		catch (Exception e)
		{
			callTimer.Log.Add(e);
			return default;
		}
	}

	private static async Task<TResult?> RunFuncAsync<TItem, TParam1, TParam2, TResult>(
		Call call,
		Func<Call, TItem, TParam1, TParam2, Task<TResult>> func,
		TItem item,
		TParam1 param1,
		TParam2 param2)
	{
		using CallTimer callTimer = call.StartTask(item?.ToString());

		try
		{
			TResult result = await func(callTimer, item, param1, param2);
			if (result == null)
			{
				callTimer.Log.Add("No result");
			}
			return result;
		}
		catch (Exception e)
		{
			callTimer.Log.Add(e);
			return default;
		}
	}

	public async Task<TResult?> FirstOrDefaultAsync<TItem, TResult>(
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		// todo: Migrate FirstOrDefault() deeper
		return (await SelectAsync(
			func,
			items,
			maxConcurrentRequests,
			maxRequestsPerSecond)).FirstOrDefault();
	}

	public async Task<TResult?> FirstOrDefaultAsync<TItem, TParam1, TResult>(
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await SelectAsync(
			func,
			items,
			param1,
			maxConcurrentRequests,
			maxRequestsPerSecond)).FirstOrDefault();
	}

	public async Task<TResult?> FirstOrDefaultAsync<TItem, TParam1, TParam2, TResult>(
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await SelectAsync(
			func,
			items,
			param1,
			param2,
			maxConcurrentRequests,
			maxRequestsPerSecond)).FirstOrDefault();
	}

	public async Task<List<TResult>> SelectAsync<TItem, TResult>(
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			func,
			items,
			maxConcurrentRequests,
			maxRequestsPerSecond)).Values.ToList();
	}

	public async Task<List<TResult>> SelectAsync<TItem, TParam1, TResult>(
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			func,
			items,
			param1,
			maxConcurrentRequests,
			maxRequestsPerSecond)).Values.ToList();
	}

	public async Task<List<TResult>> SelectAsync<TItem, TParam1, TParam2, TResult>(
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			func,
			items,
			param1,
			param2,
			maxConcurrentRequests,
			maxRequestsPerSecond)).Values.ToList();
	}

	// Call func for every item in the list using the specified parameters
	public async Task<List<TResult>> SelectAsync<TItem, TResult>(
		string name,
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			name,
			func,
			items,
			maxConcurrentRequests,
			maxRequestsPerSecond)).Values.ToList();
	}

	public async Task<List<TResult>> SelectAsync<TItem, TParam1, TResult>(
		string name,
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			name,
			func,
			items,
			param1,
			maxConcurrentRequests,
			maxRequestsPerSecond)).Values.ToList();
	}

	public async Task<List<TResult>> SelectAsync<TItem, TParam1, TParam2, TResult>(
		string name,
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			name,
			func,
			items,
			param1,
			param2,
			maxConcurrentRequests,
			maxRequestsPerSecond)).Values.ToList();
	}

	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TResult>(
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return await RunAsync(
			func.Method.Name.TrimEnd("Async").WordSpaced(),
			func,
			items,
			maxConcurrentRequests,
			maxRequestsPerSecond);
	}

	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TParam1, TResult>(
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return await RunAsync(
			func.Method.Name.TrimEnd("Async").WordSpaced(),
			func,
			items,
			param1,
			maxConcurrentRequests,
			maxRequestsPerSecond);
	}

	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TParam1, TParam2, TResult>(
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return await RunAsync(
			func.Method.Name.TrimEnd("Async").WordSpaced(),
			func,
			items,
			param1,
			param2,
			maxConcurrentRequests,
			maxRequestsPerSecond);
	}

	// Call func for every item in the list using the specified parameters
	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TResult>(
		string name,
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		using CallTimer callTimer = StartTask(items.Count, name);

		using var rateLimiter = new ConcurrentRateLimiter(maxConcurrentRequests, maxRequestsPerSecond);

		var tasks = new List<Task>();
		var results = new KeyValuePair<TItem, TResult?>[items.Count];

		foreach (var (index, item) in items.WithIndex())
		{
			var limitToken = await rateLimiter.WaitAsync();

			if (TaskInstance?.CancelToken.IsCancellationRequested == true)
			{
				limitToken.Dispose();
				Log.Add("Cancelled");
				break;
			}

			tasks.Add(Task.Run(async () =>
			{
				try
				{
					TResult? result = await RunFuncAsync(callTimer, func, item);
					results[index] = new(item, result);
				}
				catch (Exception e)
				{
					results[index] = new(item, default);
					Log.Add(e);
				}
				finally
				{
					limitToken.Dispose();
				}
			}));
		}
		await Task.WhenAll(tasks);

		return new ItemResultCollection<TItem, TResult>(results);
	}

	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TParam1, TResult>(
		string name,
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		using CallTimer callTimer = StartTask(items.Count, name);

		using var rateLimiter = new ConcurrentRateLimiter(maxConcurrentRequests, maxRequestsPerSecond);

		var tasks = new List<Task>();
		var results = new KeyValuePair<TItem, TResult?>[items.Count];
		foreach (var (index, item) in items.WithIndex())
		{
			var limitToken = await rateLimiter.WaitAsync();

			if (TaskInstance?.CancelToken.IsCancellationRequested == true)
			{
				limitToken.Dispose();
				Log.Add("Cancelled");
				break;
			}

			tasks.Add(Task.Run(async () =>
			{
				try
				{
					TResult? result = await RunFuncAsync(callTimer, func, item, param1);
					results[index] = new(item, result);
				}
				catch (Exception e)
				{
					results[index] = new(item, default);
					Log.Add(e);
				}
				finally
				{
					limitToken.Dispose();
				}
			}));
		}
		await Task.WhenAll(tasks);

		return new ItemResultCollection<TItem, TResult>(results);
	}

	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TParam1, TParam2, TResult>(
		string name,
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		using CallTimer callTimer = StartTask(items.Count, name);

		using var rateLimiter = new ConcurrentRateLimiter(maxConcurrentRequests, maxRequestsPerSecond);

		var tasks = new List<Task>();
		var results = new KeyValuePair<TItem, TResult?>[items.Count];
		foreach (var (index, item) in items.WithIndex())
		{
			var limitToken = await rateLimiter.WaitAsync();

			if (TaskInstance?.CancelToken.IsCancellationRequested == true)
			{
				limitToken.Dispose();
				Log.Add("Cancelled");
				break;
			}

			tasks.Add(Task.Run(async () =>
			{
				try
				{
					TResult? result = await RunFuncAsync(callTimer, func, item, param1, param2);
					results[index] = new(item, result);
				}
				catch (Exception e)
				{
					results[index] = new(item, default);
					Log.Add(e);
				}
				finally
				{
					limitToken.Dispose();
				}
			}));
		}
		await Task.WhenAll(tasks);

		return new ItemResultCollection<TItem, TResult>(results);
	}
}

public class ItemResultCollection<TItem, TResult>(IEnumerable<KeyValuePair<TItem, TResult?>> enumerable) :
	List<KeyValuePair<TItem, TResult?>>(enumerable)
{
	public IEnumerable<TItem> Keys => this.Select(p => p.Key);

	public IEnumerable<TResult> Values => this.Select(p => p.Value).OfType<TResult>();
}
