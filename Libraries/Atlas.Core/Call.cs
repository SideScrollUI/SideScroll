using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Atlas.Core
{
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
