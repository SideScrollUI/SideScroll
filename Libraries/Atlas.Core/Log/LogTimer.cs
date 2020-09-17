using System;
using System.Diagnostics;
using System.Threading;

namespace Atlas.Core
{
	public class LogTimer : Log, IDisposable
	{
		private Stopwatch stopwatch = new Stopwatch();
		private System.Timers.Timer timer = new System.Timers.Timer();

		public LogTimer()
		{
		}

		public LogTimer(string text, SynchronizationContext context) :
			base(text, context)
		{
			Add(text);
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
			Duration = stopwatch.ElapsedMilliseconds / 1000.0f;
			CreateEventPropertyChanged(nameof(Duration));
		}

		public void Dispose()
		{
			timer.Elapsed -= Timer_Elapsed;
			timer.Stop();
			stopwatch.Stop();
			UpdateDuration();
			
			Add("Finished", new Tag("Duration", Duration));
		}
	}
}
