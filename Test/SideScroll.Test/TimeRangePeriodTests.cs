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

	[Test]
	public void PeriodSumMilliseconds()
	{
		TimeWindow timeWindow = new()
		{
			StartTime = StartTime,
			EndTime = StartTime.AddSeconds(1),
		};
		TimeSpan periodDuration = TimeSpan.FromMilliseconds(100);

		List<TimeRangeValue> timeRangeValues = [];
		for (int i = 0; i < 10; i++)
		{
			DateTime startTime = StartTime.Add(periodDuration * i);
			DateTime endTime = startTime.Add(periodDuration);
			timeRangeValues.Add(new TimeRangeValue(startTime, endTime, 1));
		}

		var listSeries = new ListSeries(timeRangeValues)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double? total = listSeries.CalculateTotal(timeWindow);
		Assert.That(total, Is.EqualTo(10.0));
	}

	[Test]
	public void PeriodSumMicroseconds()
	{
		TimeWindow timeWindow = new()
		{
			StartTime = StartTime,
			EndTime = StartTime.AddMilliseconds(1),
		};
		TimeSpan periodDuration = TimeSpan.FromMicroseconds(100);

		List<TimeRangeValue> timeRangeValues = [];
		for (int i = 0; i < 10; i++)
		{
			DateTime startTime = StartTime.Add(periodDuration * i);
			DateTime endTime = startTime.Add(periodDuration);
			timeRangeValues.Add(new TimeRangeValue(startTime, endTime, 1));
		}

		var listSeries = new ListSeries(timeRangeValues)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double? total = listSeries.CalculateTotal(timeWindow);
		Assert.That(total, Is.EqualTo(10.0));
	}

	[Test]
	public void PeriodSumTicks()
	{
		TimeWindow timeWindow = new()
		{
			StartTime = StartTime,
			EndTime = StartTime.AddTicks(1000),
		};
		TimeSpan periodDuration = TimeSpan.FromTicks(100);

		List<TimeRangeValue> timeRangeValues = [];
		for (int i = 0; i < 10; i++)
		{
			DateTime startTime = StartTime.Add(periodDuration * i);
			DateTime endTime = startTime.Add(periodDuration);
			timeRangeValues.Add(new TimeRangeValue(startTime, endTime, 1));
		}

		var listSeries = new ListSeries(timeRangeValues)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double? total = listSeries.CalculateTotal(timeWindow);
		Assert.That(total, Is.EqualTo(10.0));
	}

	[Test]
	public void PeriodCountsTicks()
	{
		TimeWindow timeWindow = new()
		{
			StartTime = StartTime,
			EndTime = StartTime.AddTicks(1000),
		};
		TimeSpan periodDuration = TimeSpan.FromTicks(100);

		List<TimeRangeValue> timeRangeValues = [];
		for (int i = 0; i < 5; i++)
		{
			DateTime startTime = StartTime.Add(periodDuration * i * 2);
			timeRangeValues.Add(new TimeRangeValue(startTime, startTime, 1));
		}

		List<TimeRangeValue> periodCounts = timeWindow.PeriodCounts(timeRangeValues, periodDuration, true)!;

		var listSeries = new ListSeries(periodCounts)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double? total = listSeries.CalculateTotal(timeWindow);
		Assert.That(total, Is.EqualTo(5.0));
	}
}
