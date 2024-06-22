using System.Diagnostics;

namespace SideScroll.Logs;

public class LogTimer : Log, IDisposable
{
	private readonly Stopwatch _stopwatch = new();
	private readonly System.Timers.Timer _timer = new();

	public LogTimer() { }

	public LogTimer(string text, LogSettings? logSettings, Tag[] tags) :
		base(text, logSettings)
	{
		Tags = tags;

		Add(text, tags);

		InitializeTimer();
	}

	public LogTimer(LogLevel logLevel, string text, LogSettings? logSettings, Tag[] tags) :
		base(text, logSettings)
	{
		Level = logLevel;
		Tags = tags;

		Add(logLevel, text, tags);

		InitializeTimer();
	}

	private void InitializeTimer()
	{
		_stopwatch.Start();

		_timer.Interval = 1000.0;
		_timer.Elapsed += Timer_Elapsed;
		_timer.Start();
	}

	private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
	{
		UpdateDuration();
	}

	private void UpdateDuration()
	{
		Duration = _stopwatch.Elapsed;

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
