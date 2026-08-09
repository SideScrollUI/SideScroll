using NUnit.Framework;
using SideScroll.Avalonia.Charts.LiveCharts;

namespace SideScroll.Avalonia.Tests;

[Category("Charts")]
public class LiveChartSeriesTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Charts");
	}

	private static List<LiveChartPoint> CreatePoints(params (double X, double Y)[] points)
	{
		return points
			.Select(p => new LiveChartPoint(null, p.X, p.Y, null))
			.ToList();
	}

	private static List<double?> GetValues(List<LiveChartPoint> points) => points.Select(p => p.Y).ToList();

	[Test, Description("Gaps between values add a NaN so the chart breaks the line")]
	public void BinDataPointsAddsGaps()
	{
		// Bins of 1: [5, empty, empty, 7]
		List<LiveChartPoint> input = CreatePoints((0, 5), (3, 7));

		List<LiveChartPoint> output = LiveChartSeries.BinDataPoints(input, 1);

		Assert.That(GetValues(output), Is.EqualTo(new double?[] { 5, double.NaN, 7 }));
	}

	[Test, Description("Every gap adds a NaN, not just the first one")]
	public void BinDataPointsAddsAllGaps()
	{
		// Bins of 1: [5, empty, empty, 7, empty, 3]
		List<LiveChartPoint> input = CreatePoints((0, 5), (3, 7), (5, 3));

		List<LiveChartPoint> output = LiveChartSeries.BinDataPoints(input, 1);

		Assert.That(GetValues(output), Is.EqualTo(new double?[] { 5, double.NaN, 7, double.NaN, 3 }));
	}

	[Test, Description("Consecutive empty bins only add a single NaN")]
	public void BinDataPointsMergesAdjacentGaps()
	{
		List<LiveChartPoint> input = CreatePoints((0, 5), (10, 7));

		List<LiveChartPoint> output = LiveChartSeries.BinDataPoints(input, 1);

		Assert.That(GetValues(output), Is.EqualTo(new double?[] { 5, double.NaN, 7 }));
	}

	[Test]
	public void BinDataPointsSumsWithinBins()
	{
		List<LiveChartPoint> input = CreatePoints((0, 1), (0.5, 2), (1, 4));

		List<LiveChartPoint> output = LiveChartSeries.BinDataPoints(input, 1);

		Assert.That(GetValues(output), Is.EqualTo(new double?[] { 3, 4 }));
	}

	[Test, Description("A bin whose values sum to zero isn't a gap")]
	public void BinDataPointsKeepsZeroSums()
	{
		List<LiveChartPoint> input = CreatePoints((0, 5), (1, -3), (1.5, 3), (2, 7));

		List<LiveChartPoint> output = LiveChartSeries.BinDataPoints(input, 1);

		Assert.That(GetValues(output), Is.EqualTo(new double?[] { 5, 0, 7 }));
	}

	[Test, Description("Negative values align to the bin containing them, not the one after")]
	public void BinDataPointsAlignsNegativeValues()
	{
		List<LiveChartPoint> input = CreatePoints((-5, 1), (-3, 2));

		List<LiveChartPoint> output = LiveChartSeries.BinDataPoints(input, 2);

		// -5 belongs to the bin starting at -6, -3 to the bin starting at -4
		Assert.That(output.Select(p => p.X).ToList(), Is.EqualTo(new double?[] { -6, -4 }));
		Assert.That(GetValues(output), Is.EqualTo(new double?[] { 1, 2 }));
	}

	[Test]
	public void BinDataPointsEmpty()
	{
		Assert.That(LiveChartSeries.BinDataPoints([], 1), Is.Empty);
	}

	// GetDataPoints() converts a NaN Y to null so the chart draws a gap, which is what the gap
	// filled series from TimeRangeValue.FillAndMerge() is made of
	private static List<LiveChartPoint> CreatePointsWithGaps(params (double X, double? Y)[] points)
	{
		return points
			.Select(p => new LiveChartPoint(null, p.X, p.Y, null))
			.ToList();
	}

	[Test, Description(
		"A null Y is the gap GetDataPoints() creates for a NaN, and binning dereferenced it, so " +
		"turning on XBinSize threw for any gap filled series")]
	public void BinDataPointsSkipsNullValues()
	{
		List<LiveChartPoint> input = CreatePointsWithGaps((0, 5), (1, null), (2, 7));

		List<LiveChartPoint> output = LiveChartSeries.BinDataPoints(input, 1);

		// The middle bin holds only the gap, so it stays empty and becomes one
		Assert.That(GetValues(output), Is.EqualTo(new double?[] { 5, double.NaN, 7 }));
	}

	[Test, Description("A null alongside a value in the same bin leaves that value alone")]
	public void BinDataPointsIgnoresNullsSharingABin()
	{
		List<LiveChartPoint> input = CreatePointsWithGaps((0, 5), (0.5, null), (1, 7));

		List<LiveChartPoint> output = LiveChartSeries.BinDataPoints(input, 1);

		Assert.That(GetValues(output), Is.EqualTo(new double?[] { 5, 7 }));
	}

	[Test, Description("A series of nothing but gaps bins to a single break rather than throwing")]
	public void BinDataPointsHandlesOnlyNulls()
	{
		List<LiveChartPoint> input = CreatePointsWithGaps((0, null), (1, null), (2, null));

		List<LiveChartPoint> output = LiveChartSeries.BinDataPoints(input, 1);

		Assert.That(GetValues(output), Is.EqualTo(new double?[] { double.NaN }));
	}
}
