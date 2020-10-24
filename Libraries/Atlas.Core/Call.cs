using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Core
{
	[Unserialized]
	public class Call
	{
		private const int MaxRequestsPerSecond = 10;

		public string Name { get; set; } = "";
		public Log Log { get; set; }

		public Call ParentCall { get; set; }
		[Unserialized]
		public TaskInstance TaskInstance { get; set; } // Shows the Task Status and let's you stop them

		public override string ToString() => Name;

		protected Call()
		{
		}

		public Call(string name = null)
		{
			Name = name;
			Log = new Log();
		}

		public Call(Log log)
		{
			Log = log;
		}

		public Call Child([CallerMemberName] string name = "", params Tag[] tags)
		{
			Log = Log ?? new Log();
			Call call = new Call()
			{
				Name = name,
				ParentCall = this,
				TaskInstance = TaskInstance,
				Log = Log.Call(name, tags),
			};
			return call;
		}

		public CallTimer Timer([CallerMemberName] string name = "", params Tag[] tags)
		{
			Log = Log ?? new Log();
			var call = new CallTimer()
			{
				Name = name,
				ParentCall = this,
			};
			call.TaskInstance = TaskInstance?.AddSubTask(call);
			call.Log = Log.Call(name, tags);

			return call;
		}

		public CallTimer Timer(int taskCount, [CallerMemberName] string name = "", params Tag[] tags)
		{
			TaskInstance = TaskInstance ?? new TaskInstance();
			if (TaskInstance.TaskCount == 0)
				TaskInstance.TaskCount = 1;
			var allTags = tags.ToList();
			allTags.Add(new Tag("Count", taskCount));
			CallTimer timer = Timer(name, allTags.ToArray());
			timer.TaskInstance.TaskCount = taskCount;
			return timer;
		}

		// allows having progress broken down into multiple tasks
		public TaskInstance AddSubTask(string name = "")
		{
			TaskInstance = TaskInstance.AddSubTask(Child(name));
			return TaskInstance;
		}

		// allows having progress broken down into multiple tasks
		public Call AddSubCall(string name = "")
		{
			return AddSubTask(name).Call;
		}

		private async Task<T2> RunFuncAsync<T1, T2>(Call call, Func<Call, T1, Task<T2>> func, T1 item)
		{
			using (CallTimer callTimer = call.Timer(item.ToString()))
			{
				try
				{
					return await func(callTimer, item);
				}
				catch (Exception e)
				{
					callTimer.Log.Add(e);
					return default;
				}
			}
		}

		private async Task<T3> RunFuncAsync<T1, T2, T3>(Call call, Func<Call, T1, T2, Task<T3>> func, T1 item, T2 param1)
		{
			using (CallTimer callTimer = call.Timer(item.ToString()))
			{
				try
				{
					return await func(callTimer, item, param1);
				}
				catch (Exception e)
				{
					callTimer.Log.Add(e);
					return default;
				}
			}
		}

		private async Task<T4> RunFuncAsync<T1, T2, T3, T4>(Call call, Func<Call, T1, T2, T3, Task<T4>> func, T1 item, T2 param1, T3 param2)
		{
			using (CallTimer callTimer = call.Timer(item.ToString()))
			{
				try
				{
					return await func(callTimer, item, param1, param2);
				}
				catch (Exception e)
				{
					callTimer.Log.Add(e);
					return default;
				}
			}
		}

		public async Task<List<T2>> RunAsync<T1, T2>(Func<Call, T1, Task<T2>> func, List<T1> items, int maxRequestsPerSecond = MaxRequestsPerSecond)
		{
			return await RunAsync(func.Method.Name.TrimEnd("Async").WordSpaced(), func, items, maxRequestsPerSecond);
		}

		public async Task<List<T3>> RunAsync<T1, T2, T3>(Func<Call, T1, T2, Task<T3>> func, List<T1> items, T2 param1, int maxRequestsPerSecond = MaxRequestsPerSecond)
		{
			return await RunAsync(func.Method.Name.TrimEnd("Async").WordSpaced(), func, items, param1, maxRequestsPerSecond);
		}

		public async Task<List<T4>> RunAsync<T1, T2, T3, T4>(Func<Call, T1, T2, T3, Task<T4>> func, List<T1> items, T2 param1, T3 param2, int maxRequestsPerSecond = MaxRequestsPerSecond)
		{
			return await RunAsync(func.Method.Name.TrimEnd("Async").WordSpaced(), func, items, param1, param2, maxRequestsPerSecond);
		}

		// Call func for every item in the list using the specified parameters
		public async Task<List<T2>> RunAsync<T1, T2>(string name, Func<Call, T1, Task<T2>> func, List<T1> items, int maxRequestsPerSecond = MaxRequestsPerSecond)
		{
			var results = new List<T2>();
			using (CallTimer callTimer = Timer(items.Count, name))
			{
				using (var throttler = new SemaphoreSlim(maxRequestsPerSecond))
				{
					var tasks = new List<Task>();
					foreach (var item in items)
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
								T2 result = await RunFuncAsync(callTimer, func, item);
								if (result != null)
									results.Add(result);
							}
							finally
							{
								throttler.Release();
							}
						}));
					}
					await Task.WhenAll(tasks);
				}
				return results;
			}
		}

		public async Task<List<T3>> RunAsync<T1, T2, T3>(string name, Func<Call, T1, T2, Task<T3>> func, List<T1> items, T2 param1, int maxRequestsPerSecond = MaxRequestsPerSecond)
		{
			var results = new List<T3>();
			using (CallTimer callTimer = Timer(items.Count, name))
			{
				using (var throttler = new SemaphoreSlim(maxRequestsPerSecond))
				{
					var tasks = new List<Task>();
					foreach (var item in items)
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
								T3 result = await RunFuncAsync(callTimer, func, item, param1);
								if (result != null)
									results.Add(result);
							}
							finally
							{
								throttler.Release();
							}
						}));
					}
					await Task.WhenAll(tasks);
				}
				return results;
			}
		}

		public async Task<List<T4>> RunAsync<T1, T2, T3, T4>(string name, Func<Call, T1, T2, T3, Task<T4>> func, List<T1> items, T2 param1, T3 param2, int maxRequestsPerSecond = MaxRequestsPerSecond)
		{
			var results = new List<T4>();
			using (CallTimer callTimer = Timer(items.Count, name))
			{
				using (var throttler = new SemaphoreSlim(maxRequestsPerSecond))
				{
					var tasks = new List<Task>();
					foreach (var item in items)
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
								T4 result = await RunFuncAsync(callTimer, func, item, param1, param2);
								if (result != null)
									results.Add(result);
							}
							finally
							{
								throttler.Release();
							}
						}));
					}
					await Task.WhenAll(tasks);
				}
				return results;
			}
		}
	}

	public class CallTimer : Call, IDisposable
	{
		private Stopwatch _stopwatch = new Stopwatch();
		private System.Timers.Timer _timer = new System.Timers.Timer();

		public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

		public CallTimer()
		{
			_stopwatch.Start();

			_timer.Interval = 1000.0;
			_timer.Elapsed += Timer_Elapsed;
			_timer.Start();
		}

		public void Stop()
		{
			_timer.Stop();
			_stopwatch.Stop();
			_timer.Elapsed -= Timer_Elapsed;
			UpdateDuration();
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			UpdateDuration();
		}

		private void UpdateDuration()
		{
			if (Log != null)
				Log.Duration = ElapsedMilliseconds / 1000.0f;
		}

		public void Dispose()
		{
			Stop();
			TaskInstance?.SetFinished();
		}
	}
}
