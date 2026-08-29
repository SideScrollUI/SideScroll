using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LiveChartsCore;
using NUnit.Framework;
using SideScroll.Avalonia.Charts;
using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Avalonia.Utilities;
using SideScroll.Charts;

namespace SideScroll.Avalonia.Tests.Charts;

/// <summary>
/// The legend panel owns one context menu for every item in it, so the item a copy applies to has
/// to be resolved from where the menu was opened
/// </summary>
public class ChartLegendContextMenuTests
{
	private class Sample(double value)
	{
		public double Value => value;
	}

	private static TabLiveChart CreateChart()
	{
		var chartView = new ChartView("Legend");
		chartView.AddSeries("Series A", new List<Sample> { new(1), new(2) }, yPropertyName: nameof(Sample.Value));
		chartView.AddSeries("Series B", new List<Sample> { new(4) }, yPropertyName: nameof(Sample.Value));

		return new TabLiveChart(chartView, true);
	}

	private static Window ShowChart(TabLiveChart chart)
	{
		var window = new Window
		{
			Width = 800,
			Height = 600,
			Content = chart,
		};
		HeadlessWindow.ShowAndSettle(window);
		return window;
	}

	private static MenuItem GetMenuItem(TabChartLegend<ISeries> legend, string header)
	{
		var items = (IEnumerable<object>)legend.ContextMenu!.ItemsSource!;
		return items
			.OfType<MenuItem>()
			.Single(item => (string?)item.Header == header);
	}

	private static void RequestContextMenu(Control control)
	{
		control.RaiseEvent(new ContextRequestedEventArgs());
	}

	private static async Task<string?> ClickAsync(MenuItem menuItem, Visual visual)
	{
		menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

		// The handlers copy asynchronously
		Dispatcher.UIThread.RunJobs();

		return await ClipboardUtils.TryGetTextAsync(visual);
	}

	[AvaloniaTest, Description("The copy applies to the item the menu was opened on")]
	public void OpeningOnALegendItemEnablesTheItemActions()
	{
		using TabLiveChart chart = CreateChart();
		Window window = ShowChart(chart);

		TabChartLegend<ISeries> legend = chart.Legend;

		RequestContextMenu(legend.LegendItems[0]);

		Assert.That(GetMenuItem(legend, "Copy - _Name").IsEnabled, Is.True);
		Assert.That(GetMenuItem(legend, "Copy - _Row").IsEnabled, Is.True);
	}

	[AvaloniaTest, Description(
		"The menu covers the whole panel, so it opens over the empty space below the items as well, " +
		"where there's no item to copy")]
	public void OpeningOutsideOfALegendItemDisablesTheItemActions()
	{
		using TabLiveChart chart = CreateChart();
		Window window = ShowChart(chart);

		TabChartLegend<ISeries> legend = chart.Legend;

		RequestContextMenu(legend.LegendItems[0]);
		RequestContextMenu(legend);

		Assert.That(GetMenuItem(legend, "Copy - _Name").IsEnabled, Is.False);
		Assert.That(GetMenuItem(legend, "Copy - _Row").IsEnabled, Is.False);
	}

	[AvaloniaTest, Description("The name is copied without the rank prefix the label can show")]
	public async Task CopyNameCopiesTheSeriesName()
	{
		using TabLiveChart chart = CreateChart();
		Window window = ShowChart(chart);

		TabChartLegend<ISeries> legend = chart.Legend;
		TabChartLegendItem<ISeries> legendItem = legend.IdxLegendItems["Series B"];

		RequestContextMenu(legendItem);
		string? text = await ClickAsync(GetMenuItem(legend, "Copy - _Name"), legend);

		Assert.That(legendItem.TextBlock!.Text, Is.EqualTo("1. Series B"), "the label is ranked");
		Assert.That(text, Is.EqualTo("Series B"));
	}

	[AvaloniaTest, Description("The row carries the same values the whole legend copy would")]
	public async Task CopyRowCopiesTheNameAndTotal()
	{
		using TabLiveChart chart = CreateChart();
		Window window = ShowChart(chart);

		TabChartLegend<ISeries> legend = chart.Legend;
		TabChartLegendItem<ISeries> legendItem = legend.IdxLegendItems["Series B"];

		RequestContextMenu(legendItem);
		string? text = await ClickAsync(GetMenuItem(legend, "Copy - _Row"), legend);

		Assert.That(text, Does.Contain("Name: Series B"));
		Assert.That(text, Does.Contain("Total: " + legendItem.Total));
	}
}
