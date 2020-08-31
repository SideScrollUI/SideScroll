﻿using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Atlas.Core
{
	[Unserialized]
	public class Call
	{
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
			if (TaskInstance.TaskCount == 0)
				TaskInstance.TaskCount = 1;
			CallTimer timer = Timer(name, tags);
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

		public async Task<T2> RunTaskAsync<T1, T2>(Call call, T1 item, Func<Call, T1, Task<T2>> func)
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

		public async Task<T3> RunTaskAsync<T1, T2, T3>(Call call, T1 item, T2 param1, Func<Call, T1, T2, Task<T3>> func)
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

		public async Task<T4> RunTaskAsync<T1, T2, T3, T4>(Call call, Func<Call, T1, T2, T3, Task<T4>> func, T1 item, T2 param1, T3 param2)
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

		public async Task<List<T2>> RunAsync<T1, T2>(Func<Call, T1, Task<T2>> func, List<T1> items)
		{
			return await RunAsync(func.Method.Name.TrimEnd("Async").WordSpaced(), func, items);
		}

		public async Task<List<T3>> RunAsync<T1, T2, T3>(Func<Call, T1, T2, Task<T3>> func, List<T1> items, T2 param1)
		{
			return await RunAsync(func.Method.Name.TrimEnd("Async").WordSpaced(), func, items, param1);
		}

		public async Task<List<T4>> RunAsync<T1, T2, T3, T4>(Func<Call, T1, T2, T3, Task<T4>> func, List<T1> items, T2 param1, T3 param2)
		{
			return await RunAsync(func.Method.Name.TrimEnd("Async").WordSpaced(), func, items, param1, param2);
		}

		// Call func for every item in the list using the specified parameters
		public async Task<List<T2>> RunAsync<T1, T2>(string name, Func<Call, T1, Task<T2>> func, List<T1> items)
		{
			using (CallTimer callTimer = Timer(items.Count, name))
			{
				IEnumerable<Task<T2>> getTasksQuery =
					from item in items select RunTaskAsync(callTimer, item, func);

				List<Task<T2>> getResultTasks = getTasksQuery.ToList();

				var results = new List<T2>();
				while (getResultTasks.Count > 0)
				{
					Task<T2> firstFinishedTask = await Task.WhenAny(getResultTasks);
					getResultTasks.Remove(firstFinishedTask);

					T2 taskResult = await firstFinishedTask;
					if (taskResult != null)
						results.Add(taskResult);
				}
				return results;
			}
		}

		public async Task<List<T3>> RunAsync<T1, T2, T3>(string name, Func<Call, T1, T2, Task<T3>> func, List<T1> items, T2 param1)
		{
			using (CallTimer callTimer = Timer(items.Count, name))
			{
				IEnumerable<Task<T3>> getTasksQuery =
					from item in items select RunTaskAsync(callTimer, item, param1, func);

				List<Task<T3>> getResultTasks = getTasksQuery.ToList();

				var results = new List<T3>();
				while (getResultTasks.Count > 0)
				{
					Task<T3> firstFinishedTask = await Task.WhenAny(getResultTasks);
					getResultTasks.Remove(firstFinishedTask);

					T3 taskResult = await firstFinishedTask;
					if (taskResult != null)
						results.Add(taskResult);
				}
				return results;
			}
		}

		public async Task<List<T4>> RunAsync<T1, T2, T3, T4>(string name, Func<Call, T1, T2, T3, Task<T4>> func, List<T1> items, T2 param1, T3 param2)
		{
			using (CallTimer callTimer = Timer(items.Count, name))
			{
				IEnumerable<Task<T4>> getTasksQuery =
					from item in items select RunTaskAsync(callTimer, func, item, param1, param2);

				List<Task<T4>> getResultTasks = getTasksQuery.ToList();

				var results = new List<T4>();
				while (getResultTasks.Count > 0)
				{
					Task<T4> firstFinishedTask = await Task.WhenAny(getResultTasks);
					getResultTasks.Remove(firstFinishedTask);

					T4 taskResult = await firstFinishedTask;
					if (taskResult != null)
						results.Add(taskResult);
				}
				return results;
			}
		}
	}

	public class CallTimer : Call, IDisposable
	{
		private Stopwatch stopwatch = new Stopwatch();
		private System.Timers.Timer timer = new System.Timers.Timer();
		public long ElapsedMilliseconds => stopwatch.ElapsedMilliseconds;

		public CallTimer()
		{
			stopwatch.Start();

			timer.Interval = 1000.0;
			timer.Elapsed += Timer_Elapsed;
			timer.Start();
		}

		public void Stop()
		{
			timer.Stop();
			stopwatch.Stop();
			timer.Elapsed -= Timer_Elapsed;
			UpdateDuration();
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			UpdateDuration();
		}

		private void UpdateDuration()
		{
			if (Log != null)
				Log.Duration = stopwatch.ElapsedMilliseconds / 1000.0f;
		}

		public void Dispose()
		{
			TaskInstance?.SetFinished();
			Stop();
		}
	}
}
