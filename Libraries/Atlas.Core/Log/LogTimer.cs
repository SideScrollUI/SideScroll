using System;
using System.Diagnostics;

namespace Atlas.Core
{
	public class LogTimer : Log, IDisposable
	{
		private Stopwatch _stopwatch = new Stopwatch();
		private System.Timers.Timer _timer = new System.Timers.Timer();

		public LogTimer()
		{
		}

		public LogTimer(string text, LogSettings logSettings) :
			base(text, logSettings)
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
			_timer.Dispose();
			_stopwatch.Stop();
			UpdateDuration();
			
			Add("Finished", new Tag("Duration", Duration));
		}
	}
}
