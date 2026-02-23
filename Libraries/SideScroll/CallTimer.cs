using System.Diagnostics;
using System.Timers;

namespace SideScroll;

/// <summary>
/// Tracks elapsed time for a call and logs duration updates at regular intervals.
/// </summary>
public class CallTimer : Call, IDisposable
{
	private readonly Stopwatch _stopwatch = new();
	private readonly System.Timers.Timer _timer = new();

	/// <summary>
	/// Gets or sets whether this timer is associated with a task instance.
	/// </summary>
	public bool IsTask { get; set; }

	/// <summary>
	/// Gets the elapsed time in milliseconds since the timer started.
	/// </summary>
	public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

	/// <summary>
	/// Initializes a new instance of the CallTimer class and starts timing.
	/// </summary>
	public CallTimer()
	{
		_stopwatch.Start();

		_timer.Interval = 1000.0;
		_timer.Elapsed += Timer_Elapsed;
		_timer.Start();
	}

	/// <summary>
	/// Stops the timer and logs the final duration.
	/// </summary>
	public void Stop()
	{
		_stopwatch.Stop();

		_timer.Stop();
		_timer.Elapsed -= Timer_Elapsed;

		UpdateDuration();

		if (TaskInstance != null && IsTask)
		{
			TaskInstance.SetFinished();
		}
		else
		{
			Log.Add("Finished", new Tag("Duration", _stopwatch.Elapsed));
		}
	}

	private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
	{
		UpdateDuration();
	}

	private void UpdateDuration()
	{
		Log.Duration = _stopwatch.Elapsed;
	}

	public void Dispose()
	{
		Stop();

		_timer.Dispose();
	}
}
