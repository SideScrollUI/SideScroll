using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using LiveChartsCore;
using NUnit.Framework;
using SideScroll.Avalonia.Charts;
using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Charts;

namespace SideScroll.Avalonia.Tests.Charts;

/// <summary>
/// A legend item lays its name out in a star column, so a name too long to fit has to be trimmed
/// rather than widen the row past the legend and carry the total column out with it
/// </summary>
public class ChartLegendLayoutTests
{
	private const string LongName = "A very long name that goes on and on and eventually gets truncated";
	private const string ShortName = "Short Name";

	private class Sample(double value)
	{
		public double Value => value;
	}

	private static TabLiveChart CreateChart()
	{
		var chartView = new ChartView("Legend")
		{
			LegendPosition = ChartLegendPosition.Right,
			ShowOrder = true,
		};
		chartView.AddSeries(LongName, new List<Sample> { new(562) }, yPropertyName: nameof(Sample.Value));
		chartView.AddSeries(ShortName, new List<Sample> { new(517) }, yPropertyName: nameof(Sample.Value));

		var chart = new TabLiveChart(chartView, true);
		var window = new Window
		{
			Width = 1000,
			Height = 600,
			Content = chart,
		};
		HeadlessWindow.ShowAndSettle(window);
		return chart;
	}

	private static TabChartLegendItem<ISeries> GetItem(TabChartLegend<ISeries> legend, string name)
	{
		return legend.LegendItems.Single(item => item.ToString() == name);
	}

	private static double GetTotalRight(TabChartLegendItem<ISeries> item)
	{
		TextBlock total = item.TextBlockTotal!;
		return total.TranslatePoint(new Point(total.Bounds.Width, 0), item)!.Value.X;
	}

	[AvaloniaTest]
	public void TotalColumnStaysAlignedWhenNameIsTrimmed()
	{
		TabLiveChart chart = CreateChart();

		Assert.That(GetTotalRight(GetItem(chart.Legend, LongName)),
			Is.EqualTo(GetTotalRight(GetItem(chart.Legend, ShortName))));
	}

	[AvaloniaTest]
	public void TotalColumnStaysInsideTheRowWhenNameIsTrimmed()
	{
		TabLiveChart chart = CreateChart();

		TabChartLegendItem<ISeries> item = GetItem(chart.Legend, LongName);

		Assert.That(GetTotalRight(item), Is.LessThanOrEqualTo(item.Bounds.Width));
	}
}
