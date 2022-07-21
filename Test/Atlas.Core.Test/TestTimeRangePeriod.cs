using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Atlas.Core.Test;

[Category("Core")]
public class TestTimeRangePeriod : TestBase
{
	private static readonly DateTime StartTime = new(2000, 1, 1);

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	[Test]
	public void PeriodCountsMergePointsTotal()
	{
		var timeWindow = new TimeWindow()
		{
			StartTime = StartTime,
			EndTime = StartTime.AddMinutes(1),
		};
		TimeSpan periodDuration = TimeSpan.FromSeconds(10);

		var timeRangeValues = new List<TimeRangeValue>();
		for (int i = 0; i < 3; i++)
		{
			DateTime startTime = StartTime.AddSeconds(5 + i * 10);
			timeRangeValues.Add(new TimeRangeValue(startTime, startTime, 1));
		}

		List<TimeRangeValue> periodSums = timeWindow.PeriodCounts(timeRangeValues, periodDuration, true)!;

		var listSeries = new ListSeries(periodSums)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double total = listSeries.CalculateTotal(timeWindow);
		Assert.AreEqual(3.0, total);
	}

	[Test]
	public void PeriodCountsBeforeTimeWindow()
	{
		var timeWindow = new TimeWindow()
		{
			StartTime = StartTime,
			EndTime = StartTime.AddMinutes(1),
		};
		TimeSpan periodDuration = TimeSpan.FromSeconds(10);

		var timeRangeValues = new List<TimeRangeValue>();
		for (int i = 0; i < 3; i++)
		{
			DateTime startTime = StartTime.AddSeconds(-15 + i * 10);
			timeRangeValues.Add(new TimeRangeValue(startTime, startTime, 1));
		}

		List<TimeRangeValue> periodSums = timeWindow.PeriodCounts(timeRangeValues, periodDuration, true)!;

		var listSeries = new ListSeries(periodSums)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double total = listSeries.CalculateTotal(timeWindow);
		Assert.AreEqual(1.0, total);
	}
}
