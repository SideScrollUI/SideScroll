using System.Diagnostics;

namespace SideScroll.Logs;

/// <summary>
/// A log entry that tracks elapsed time and updates periodically until disposed
/// </summary>
public class LogTimer : Log, IDisposable
{
	private readonly Stopwatch _stopwatch = new();
	private readonly System.Timers.Timer _timer = new();

	/// <summary>
	/// Creates a new log timer with default settings
	/// </summary>
	public LogTimer() { }

	/// <summary>
	/// Creates a new log timer with the specified text, settings, and tags
	/// </summary>
	public LogTimer(string text, LogSettings? logSettings, Tag[] tags) :
		base(text, logSettings)
	{
		Tags = tags;

		Add(text, tags);

		InitializeTimer();
	}

	/// <summary>
	/// Creates a new log timer with the specified log level, text, settings, and tags
	/// </summary>
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

		NotifyPropertyChanged(nameof(Duration));
	}

	/// <summary>
	/// Stops the timer, updates the final duration, and logs completion
	/// </summary>
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
