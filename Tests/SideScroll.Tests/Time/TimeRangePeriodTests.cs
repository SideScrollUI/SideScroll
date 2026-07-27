using NUnit.Framework;
using SideScroll.Collections;
using SideScroll.Time;

namespace SideScroll.Tests.Time;

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

	[Test, Description("Count series group each period by item count rather than averaging item values")]
	public void ListSeriesGroupByPeriodCounts()
	{
		TimeSpan periodDuration = TimeSpan.FromMinutes(1);
		TimeWindow timeWindow = new(StartTime, StartTime.Add(periodDuration));
		List<TimeRangeValue> values =
		[
			new(StartTime, StartTime, 10),
			new(StartTime.AddSeconds(10), StartTime.AddSeconds(10), 20),
		];
		var series = new ListSeries(values)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Count,
		};

		List<TimeRangeValue>? grouped = series.GroupByPeriod(timeWindow);

		Assert.That(grouped, Has.Count.EqualTo(1));
		Assert.That(grouped![0].Value, Is.EqualTo(2));
	}

	[Test, Description("CalculateTotal retains fractional values above fifty")]
	public void ListSeriesCalculateTotalDoesNotFloorLargeValues()
	{
		var series = new ListSeries(new List<double> { 50.75 })
		{
			SeriesType = SeriesType.Sum,
		};

		Assert.That(series.CalculateTotal(), Is.EqualTo(50.75));
		Assert.That(series.Total, Is.EqualTo(50.75));
	}

	[Test, Description("A point exactly at the inclusive window start participates in min and max totals")]
	public void TotalMinMaxIncludePointAtWindowStart()
	{
		TimeWindow window = new(StartTime, StartTime.AddMinutes(1));
		List<TimeRangeValue> values =
		[
			new(StartTime, StartTime, 7),
			new(StartTime.AddSeconds(10), StartTime.AddSeconds(10), 10),
		];

		Assert.That(TimeRangePeriod.TotalMinimum(values, window), Is.EqualTo(7));
		Assert.That(TimeRangePeriod.TotalMaximum(values, window), Is.EqualTo(10));
	}

	[Test, Description("Tag values are deduplicated by exact value rather than substring")]
	public void TagsKeepDistinctSubstringValues()
	{
		TimeRangePeriod period = new()
		{
			AllTags =
			[
				new Tag("Name", "foobar"),
				new Tag("Name", "foo"),
				new Tag("Name", "foobar"),
			],
		};

		Assert.That(period.Tags, Has.Count.EqualTo(1));
		Assert.That(period.Tags[0].Value, Is.EqualTo("foobar, foo"));
	}

	[Test, Description("Duplicate non-string tags retain every distinct value exactly once")]
	public void TagsKeepDistinctNonStringValues()
	{
		TimeRangePeriod period = new()
		{
			AllTags =
			[
				new Tag("Status", 1),
				new Tag("Status", 2),
				new Tag("Status", 1),
			],
		};

		Assert.That(period.Tags, Has.Count.EqualTo(1));
		Assert.That(period.Tags[0].Value, Is.EqualTo("1, 2"));
	}

	[Test]
	public void PeriodSumsDifferentlyAlignedTimeWindows()
	{
		// Test that ListSeries sums calculate correctly for differently aligned time windows
		// Each window gets its own set of TimeRangeValues aligned to its boundaries
		DateTime baseStart = new(2025, 12, 19, 0, 0, 0);
		DateTime baseEnd = new(2026, 1, 16, 0, 0, 0);
		TimeSpan periodDuration = TimeSpan.FromHours(12);

		// Test with aligned window (0:00:00)
		TimeWindow timeWindow1 = new()
		{
			StartTime = baseStart,
			EndTime = baseEnd,
		};

		// Create time range values aligned to window1 boundaries
		List<TimeRangeValue> timeRangeValues1 = [];
		DateTime current1 = timeWindow1.StartTime;
		while (current1 < timeWindow1.EndTime)
		{
			DateTime endTime = current1.Add(periodDuration);
			if (endTime > timeWindow1.EndTime)
				endTime = timeWindow1.EndTime;
			timeRangeValues1.Add(new TimeRangeValue(current1, endTime, 1));
			current1 = current1.Add(periodDuration);
		}

		var listSeries1 = new ListSeries(timeRangeValues1)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double? total1 = listSeries1.CalculateTotal(timeWindow1);

		// Test with offset window (0:01:00)
		TimeWindow timeWindow2 = new()
		{
			StartTime = baseStart.AddMinutes(1),
			EndTime = baseEnd.AddMinutes(1),
		};

		// Create time range values aligned to window2 boundaries
		List<TimeRangeValue> timeRangeValues2 = [];
		DateTime current2 = timeWindow2.StartTime;
		while (current2 < timeWindow2.EndTime)
		{
			DateTime endTime = current2.Add(periodDuration);
			if (endTime > timeWindow2.EndTime)
				endTime = timeWindow2.EndTime;
			timeRangeValues2.Add(new TimeRangeValue(current2, endTime, 1));
			current2 = current2.Add(periodDuration);
		}

		var listSeries2 = new ListSeries(timeRangeValues2)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double? total2 = listSeries2.CalculateTotal(timeWindow2);

		// Both time windows should calculate the same total sum
		// since they cover the same duration with the same amount of data
		Assert.That(total1, Is.Not.Null, "First time window total should not be null");
		Assert.That(total2, Is.Not.Null, "Second time window total should not be null");
		Assert.That(timeRangeValues1.Count, Is.EqualTo(timeRangeValues2.Count),
			"Both windows should have the same number of time range values");
		Assert.That(total2, Is.EqualTo(total1),
			$"Differently aligned time windows should produce the same sum. Window1 (0:00): {total1}, Window2 (0:01): {total2}");

		// Verify the expected count matches the number of 12-hour periods
		int expectedCount = timeRangeValues1.Count;
		Assert.That(total1, Is.EqualTo(expectedCount),
			$"Total should equal the number of time range values: {expectedCount}");
	}

	[Test]
	public void PeriodSumsFixedDataDifferentWindows()
	{
		// Test with TimeRangeValues at fixed period boundaries (midnight/noon)
		// but TimeWindows offset by a minute
		DateTime baseStart = new(2025, 12, 19, 0, 0, 0);
		DateTime baseEnd = new(2026, 1, 16, 0, 0, 0);
		TimeSpan periodDuration = TimeSpan.FromHours(12);

		// Create time range values at fixed 12-hour period boundaries
		// These represent data sampled at consistent times (e.g., midnight and noon each day)
		List<TimeRangeValue> timeRangeValues = [];
		DateTime current = baseStart;
		while (current < baseEnd)
		{
			DateTime endTime = current.Add(periodDuration);
			if (endTime > baseEnd)
				endTime = baseEnd;
			timeRangeValues.Add(new TimeRangeValue(current, endTime, 1));
			current = current.Add(periodDuration);
		}

		// Test with aligned window (0:00:00)
		TimeWindow timeWindow1 = new()
		{
			StartTime = baseStart,
			EndTime = baseEnd,
		};

		var listSeries1 = new ListSeries(timeRangeValues)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double? total1 = listSeries1.CalculateTotal(timeWindow1);

		// Test with offset window (0:01:00)
		TimeWindow timeWindow2 = new()
		{
			StartTime = baseStart.AddMinutes(1),
			EndTime = baseEnd.AddMinutes(1),
		};

		var listSeries2 = new ListSeries(timeRangeValues)
		{
			PeriodDuration = periodDuration,
			SeriesType = SeriesType.Sum,
		};
		double? total2 = listSeries2.CalculateTotal(timeWindow2);

		// Both windows view the same data set
		// Window1 is perfectly aligned and gets full credit for all values
		// Window2 is offset by 1 minute, so it misses 1 minute of the first TimeRangeValue
		Assert.That(total1, Is.Not.Null, "First time window total should not be null");
		Assert.That(total2, Is.Not.Null, "Second time window total should not be null");

		// CalculateTotal and GetTotal both preserve the exact aggregate
		double? rawTotal2 = listSeries2.GetTotal(timeWindow2);
		double expectedWindow2Raw = 55.0 + (11.0 * 60 + 59) / (12.0 * 60); // 55 + 719/720 = 55.99861...

		// The raw total should be proportionally less
		Assert.That(rawTotal2, Is.EqualTo(expectedWindow2Raw).Within(0.01),
			$"Window2 raw sum should be proportionally less. Expected: {expectedWindow2Raw}, Actual: {rawTotal2}");

		// The aligned window receives the full sum; the offset window keeps its fractional sum
		Assert.That(total1, Is.EqualTo(56), "Window1 should equal 56");
		Assert.That(total2, Is.EqualTo(expectedWindow2Raw).Within(0.01),
			"Window2 should retain its proportional fractional sum");
		Assert.That(total1, Is.GreaterThan(total2!),
			"Window1 (aligned) should have a higher sum than Window2 (offset)");

		// Verify the expected count
		int expectedCount = timeRangeValues.Count;
		Assert.That(total1, Is.EqualTo(expectedCount),
			$"Total should equal the number of time range values: {expectedCount}");
	}
}
