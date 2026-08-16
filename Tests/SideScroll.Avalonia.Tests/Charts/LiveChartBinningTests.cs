using NUnit.Framework;
using SideScroll.Avalonia.Charts.LiveCharts;

namespace SideScroll.Avalonia.Tests.Charts;

/// <summary>
/// Binning divides a series by its own X range, so the bin count comes from the data rather than
/// from anything bounded
/// </summary>
public class LiveChartBinningTests
{
	private static List<LiveChartPoint> Points(params (double X, double Y)[] values)
		=> [.. values.Select(v => new LiveChartPoint(null, v.X, v.Y, null))];

	[Test, Description(
		"A range too large to divide into a representable number of bins saturated the conversion " +
		"to int, and the increment after it made the allocation size negative")]
	public void RangeTooLargeToCountDoesNotThrow()
	{
		// A millisecond bin over a range of 1e12, two timestamps about 31 years apart
		List<LiveChartPoint> binned = LiveChartSeries.BinDataPoints(Points((0, 1), (1e12, 2)), 0.001);

		Assert.That(binned, Has.Count.LessThanOrEqualTo(LiveChartSeries.MaxBins + 1));
		Assert.That(binned, Is.Not.Empty);
	}

	[Test, Description(
		"A count that fits in an int still allocated two arrays for every bin, so a billion of " +
		"them claimed gigabytes. The output collapses empty bins either way, so the allocation is " +
		"what tells the two apart")]
	public void RangeLargerThanTheCapIsWidenedToFit()
	{
		long before = GC.GetTotalAllocatedBytes(precise: true);

		List<LiveChartPoint> binned = LiveChartSeries.BinDataPoints(Points((0, 1), (1e9, 2)), 1);

		long allocated = GC.GetTotalAllocatedBytes(precise: true) - before;

		// A billion bins is 8 GB of doubles and 4 GB of counts
		Assert.That(allocated, Is.LessThan(100_000_000), $"allocated {allocated / 1_000_000} MB");
		Assert.That(binned, Has.Count.LessThanOrEqualTo(LiveChartSeries.MaxBins + 1));
	}

	[Test, Description("Widening keeps the bins aligned to the same origin, so the values still land in them")]
	public void WidenedBinsKeepTheirValues()
	{
		List<LiveChartPoint> binned = LiveChartSeries.BinDataPoints(Points((0, 5), (1e9, 7)), 1);

		Assert.That(binned.First().Y, Is.EqualTo(5), "the first point is in the first bin");
		Assert.That(binned.Last().Y, Is.EqualTo(7), "the last point is in the last bin");

		double total = binned
			.Where(p => p.Y is { } y && !double.IsNaN(y))
			.Sum(p => p.Y!.Value);
		Assert.That(total, Is.EqualTo(12), "no value is lost or double counted");
	}

	[Test, Description("A NaN or infinite X divides into no meaningful number of bins")]
	public void NonFiniteRangeReturnsThePointsUnbinned()
	{
		List<LiveChartPoint> points = Points((0, 1), (double.PositiveInfinity, 2));

		Assert.That(LiveChartSeries.BinDataPoints(points, 1), Is.SameAs(points));
	}

	[Test, Description("A size that already fits is left alone, binning is unchanged for ordinary data")]
	public void OrdinaryRangeIsBinnedUnchanged()
	{
		List<LiveChartPoint> binned = LiveChartSeries.BinDataPoints(Points((0, 1), (4, 2), (9, 3)), 1);

		// Each run of empty bins collapses to the one NaN that breaks the line
		Assert.That(binned.Select(p => p.Y), Is.EqualTo(new double?[]
		{
			1, double.NaN, 2, double.NaN, 3,
		}));
	}

	[Test, Description("Values sharing a bin are summed, which widening must not change")]
	public void PointsInTheSameBinAreSummed()
	{
		List<LiveChartPoint> binned = LiveChartSeries.BinDataPoints(Points((0, 1), (0.5, 2)), 1);

		Assert.That(binned, Has.Count.EqualTo(1));
		Assert.That(binned.First().Y, Is.EqualTo(3));
	}

	[Test]
	public void FitBinSizeLeavesAFittingSizeUnchanged()
	{
		Assert.That(LiveChartSeries.FitBinSize(1000, 1), Is.EqualTo(1));
		Assert.That(LiveChartSeries.FitBinSize(0, 1), Is.EqualTo(1));
		Assert.That(LiveChartSeries.FitBinSize(-5, 1), Is.EqualTo(1));
	}

	[Test]
	public void FitBinSizeWidensAnOversizedCount()
	{
		double fitted = LiveChartSeries.FitBinSize(1e9, 1);

		Assert.That(fitted, Is.GreaterThan(1));
		Assert.That(Math.Floor(1e9 / fitted) + 1, Is.LessThanOrEqualTo(LiveChartSeries.MaxBins + 1));
	}

	[Test]
	public void MaxBinsRejectsANonPositiveValue()
	{
		Assert.That(() => LiveChartSeries.MaxBins = 0, Throws.TypeOf<ArgumentOutOfRangeException>());
		Assert.That(() => LiveChartSeries.MaxBins = -1, Throws.TypeOf<ArgumentOutOfRangeException>());
		Assert.That(LiveChartSeries.MaxBins, Is.EqualTo(10_000), "unchanged by a rejected value");
	}
}
