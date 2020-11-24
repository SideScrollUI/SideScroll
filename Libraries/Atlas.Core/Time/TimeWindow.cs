using System;

namespace Atlas.Core
{
	public class TimeWindow
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TimeSpan Duration => EndTime.Subtract(StartTime);

		public TimeWindow Selection { get; set; } // For zooming in

		public event EventHandler<TimeWindowEventArgs> OnSelectionChanged;

		public override string ToString() => DateTimeUtils.FormatTimeRange(StartTime, EndTime);

		public TimeWindow() { }

		public TimeWindow(DateTime startTime, DateTime endTime)
		{
			StartTime = startTime;
			EndTime = endTime;
		}

		public void Select(TimeWindow timeWindow)
		{
			Selection = timeWindow;
			OnSelectionChanged?.Invoke(this, new TimeWindowEventArgs(timeWindow ?? this));
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
}
