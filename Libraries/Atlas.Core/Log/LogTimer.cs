using System;
using System.Diagnostics;
using System.Threading;

namespace Atlas.Core
{
	public class LogTimer : Log, IDisposable
	{
		private Stopwatch _stopwatch = new Stopwatch();
		private System.Timers.Timer _timer = new System.Timers.Timer();

		public LogTimer()
		{
		}

		public LogTimer(string text, SynchronizationContext context) :
			base(text, context)
		{
			Add(text);
			_stopwatch.Start();

			_timer.Interval = 1000.0;
			_timer.Elapsed += Timer_Elapsed;
			_timer.Start();
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			UpdateDuration();
		}

		private void UpdateDuration()
		{
			Duration = _stopwatch.ElapsedMilliseconds / 1000.0f;
			CreateEventPropertyChanged(nameof(Duration));
		}

		public void Dispose()
		{
			_timer.Elapsed -= Timer_Elapsed;
			_timer.Stop();
			_stopwatch.Stop();
			UpdateDuration();
			
			Add("Finished", new Tag("Duration", Duration));
		}
	}
}
