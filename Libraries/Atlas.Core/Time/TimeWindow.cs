using Atlas.Extensions;
using System;
using System.Collections.Generic;

namespace Atlas.Core;

public class TimeWindow
{
	public string Name { get; set; }

	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }

	public TimeSpan Duration => EndTime.Subtract(StartTime);

	public TimeWindow Selection { get; set; } // For zooming in

	public event EventHandler<TimeWindowEventArgs> OnSelectionChanged;

	public override string ToString() => Name ?? Selection?.ToString() ?? DateTimeUtils.FormatTimeRange(StartTime, EndTime);

	public TimeWindow() { }

	public TimeWindow(DateTime startTime, DateTime endTime, string name = null)
	{
		StartTime = startTime;
		EndTime = endTime;
		Name = name;
	}

	public void Select(TimeWindow timeWindow)
	{
		Selection = timeWindow;
		OnSelectionChanged?.Invoke(this, new TimeWindowEventArgs(timeWindow ?? this));
	}

	public TimeWindow Trim()
	{
		return Trim(Duration.PeriodDuration());
	}

	public TimeWindow Trim(long ticks)
	{
		StartTime = StartTime.Trim(ticks);
		EndTime = EndTime.Trim(ticks);
		return this;
	}

	public TimeWindow Trim(TimeSpan timeSpan)
	{
		return Trim(timeSpan.Ticks);
	}

	public void Update(TimeWindow timeWindow)
	{
		Name = timeWindow.Name;
		StartTime = timeWindow.StartTime;
		EndTime = timeWindow.EndTime;
		Selection = timeWindow.Selection;
	}

	public List<TimeRangeValue> PeriodAverages(List<TimeRangeValue> timeRangeValues, TimeSpan periodDuration)
	{
		return TimeRangePeriod.PeriodAverages(timeRangeValues, this, periodDuration);
	}

	public List<TimeRangeValue> PeriodSums(List<TimeRangeValue> timeRangeValues, TimeSpan periodDuration)
	{
		return TimeRangePeriod.PeriodSums(timeRangeValues, this, periodDuration);
	}

	public List<TimeRangeValue> PeriodCounts(List<TimeRangeValue> timeRangeValues, TimeSpan periodDuration, bool addGaps = false)
	{
		return TimeRangePeriod.PeriodCounts(timeRangeValues, this, periodDuration, addGaps);
	}
}

public class TimeWindowEventArgs : EventArgs
{
	public TimeWindow TimeWindow { get; set; }

	public TimeWindowEventArgs(TimeWindow timeWindow)
	{
		TimeWindow = timeWindow;
	}
}
