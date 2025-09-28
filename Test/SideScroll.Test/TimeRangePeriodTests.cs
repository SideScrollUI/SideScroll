using NUnit.Framework;
using SideScroll.Collections;
using SideScroll.Time;

namespace SideScroll.Test;

[Category("Core")]
public class TimeRangePeriodTests : BaseTest
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
		TimeWindow timeWindow = new()
		{
			StartTime = StartTime,
			EndTime = StartTime.AddMinutes(1),
		};
		TimeSpan periodDuration = TimeSpan.FromSeconds(10);

		List<TimeRangeValue> timeRangeValues = [];
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
		double? total = listSeries.CalculateTotal(timeWindow);
		Assert.That(total, Is.EqualTo(3.0));
	}

	[Test]
	public void PeriodCountsBeforeTimeWindow()
	{
		TimeWindow timeWindow = new()
		{
			StartTime = StartTime,
			EndTime = StartTime.AddMinutes(1),
		};
		TimeSpan periodDuration = TimeSpan.FromSeconds(10);

		List<TimeRangeValue> timeRangeValues = [];
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
		double? total = listSeries.CalculateTotal(timeWindow);
		Assert.That(total, Is.EqualTo(1.0));
	}
}
