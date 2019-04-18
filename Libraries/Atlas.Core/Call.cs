using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Atlas.Core
{
	public class Call
	{
		public string Name { get; set; } = "";
		public Log log;

		public Call ParentCall { get; set; }
		public TaskInstance taskInstance; // Shows the Task Status and let's you stop them
		
		protected Call()
		{
		}

		public Call(string name = null)
		{
			Name = name;
			log = new Log();
		}

		public Call(Log log)
		{
			this.log = log;
		}

		public override string ToString()
		{
			return Name;
		}

		public Call Child([CallerMemberName] string name = "", params Tag[] tags)
		{
			Call call = new Call();
			call.Name = name;
			call.ParentCall = this;
			call.taskInstance = taskInstance;
			call.log = log.Call(name, tags);

			return call;
		}

		public CallTimer Timer([CallerMemberName] string name = "", params Tag[] tags)
		{
			CallTimer call = new CallTimer();
			call.Name = name;
			call.ParentCall = this;
			call.taskInstance = taskInstance;
			call.log = log.Call(name, tags);

			return call;
		}

		// allows having progress broken down into multiple tasks
		public TaskInstance AddSubTask(string name = "")
		{
			taskInstance = taskInstance.AddSubTask(this, name);
			return taskInstance;
		}

		// allows having progress broken down into multiple tasks
		public Call AddSubCall()
		{
			return AddSubTask().call;
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

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			UpdateDuration();
		}

		private void UpdateDuration()
		{
			log.Duration = stopwatch.ElapsedMilliseconds / 1000.0f;
		}

		public void Dispose()
		{
			timer.Stop();
			stopwatch.Stop();
			UpdateDuration();
		}
	}
}
/*
*/
