using SideScroll.Utilities;
using SideScroll.Extensions;

namespace SideScroll.Time;

/// <summary>
/// Represents a window of time with start and end boundaries
/// </summary>
public class TimeWindow
{
	/// <summary>
	/// Gets or sets the name of this time window
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the start time of this window
	/// </summary>
	public DateTime StartTime { get; set; }
	
	/// <summary>
	/// Gets or sets the end time of this window
	/// </summary>
	public DateTime EndTime { get; set; }

	/// <summary>
	/// Gets the duration of this time window
	/// </summary>
	public TimeSpan Duration => EndTime.Subtract(StartTime);

	/// <summary>
	/// Gets or sets the selected sub-window for zooming
	/// </summary>
	public TimeWindow? Selection { get; set; }

	/// <summary>
	/// Occurs when the selection changes
	/// </summary>
	public event EventHandler<TimeWindowEventArgs>? OnSelectionChanged;

	public override string ToString() => Name ?? Selection?.ToString() ?? DateTimeUtils.FormatTimeRange(StartTime, EndTime);

	/// <summary>
	/// Initializes a new instance of the TimeWindow class
	/// </summary>
	public TimeWindow() { }

	/// <summary>
	/// Initializes a new instance with specified start and end times
	/// </summary>
	public TimeWindow(DateTime startTime, DateTime endTime, string? name = null)
	{
		StartTime = startTime;
		EndTime = endTime;
		Name = name;
	}

	/// <summary>
	/// Sets the selection to a specified time window and raises the selection changed event
	/// </summary>
	public void Select(TimeWindow? timeWindow)
	{
		Selection = timeWindow;
		OnSelectionChanged?.Invoke(this, new TimeWindowEventArgs(timeWindow ?? this));
	}

	/// <summary>
	/// Trims the time window boundaries to align with the period duration
	/// </summary>
	public TimeWindow Trim()
	{
		return Trim(Duration.PeriodDuration());
	}

	/// <summary>
	/// Trims the time window boundaries to align with the specified tick interval
	/// </summary>
	public TimeWindow Trim(long ticks)
	{
		StartTime = StartTime.Trim(ticks);
		EndTime = EndTime.Trim(ticks);
		return this;
	}

	/// <summary>
	/// Trims the time window boundaries to align with the specified time span
	/// </summary>
	public TimeWindow Trim(TimeSpan timeSpan)
	{
		return Trim(timeSpan.Ticks);
	}

	/// <summary>
	/// Updates this time window with values from another time window
	/// </summary>
	public void Update(TimeWindow timeWindow)
	{
		Name = timeWindow.Name;
		StartTime = timeWindow.StartTime;
		EndTime = timeWindow.EndTime;
		Selection = timeWindow.Selection;
	}

	/// <summary>
	/// Calculates the average value for each period within this time window
	/// </summary>
	public List<TimeRangeValue>? PeriodAverages(List<TimeRangeValue> timeRangeValues, TimeSpan periodDuration)
	{
		return TimeRangePeriod.PeriodAverages(timeRangeValues, this, periodDuration);
	}

	/// <summary>
	/// Calculates the sum of values for each period within this time window
	/// </summary>
	public List<TimeRangeValue>? PeriodSums(List<TimeRangeValue> timeRangeValues, TimeSpan periodDuration)
	{
		return TimeRangePeriod.PeriodSums(timeRangeValues, this, periodDuration);
	}

	/// <summary>
	/// Calculates the count of values for each period within this time window
	/// </summary>
	/// <param name="fillAndMerge">Whether to add NaN gaps between periods with no data and merge duplicate values</param>
	public List<TimeRangeValue>? PeriodCounts(List<TimeRangeValue> timeRangeValues, TimeSpan periodDuration, bool fillAndMerge = false)
	{
		return TimeRangePeriod.PeriodCounts(timeRangeValues, this, periodDuration, fillAndMerge);
	}
}

/// <summary>
/// Provides data for time window selection change events
/// </summary>
public class TimeWindowEventArgs(TimeWindow timeWindow) : EventArgs
{
	/// <summary>
	/// Gets the time window associated with this event
	/// </summary>
	public TimeWindow TimeWindow => timeWindow;
}
