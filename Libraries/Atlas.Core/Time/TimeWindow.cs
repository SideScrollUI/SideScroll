using System;

namespace Atlas.Core
{
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
