using Atlas.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using OxyPlot.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.UI.Avalonia.Charts;

public class TabControlChartLegend : Grid
{
	public TabControlChart TabControlChart;
	public PlotView? PlotView => TabControlChart.PlotView;
	public ListGroup ListGroup => TabControlChart.ListGroup;

	public List<TabChartLegendItem> LegendItems = new();
	private readonly Dictionary<string, TabChartLegendItem> _idxLegendItems = new();

	private readonly ScrollViewer _scrollViewer;
	private readonly WrapPanel _wrapPanel;
	private readonly TextBlock? _textBlockTotal;

	public event EventHandler<EventArgs>? OnSelectionChanged;
	public event EventHandler<EventArgs>? OnVisibleChanged;

	public override string? ToString() => ListGroup.ToString();

	public TabControlChartLegend(TabControlChart tabControlChart)
	{
		TabControlChart = tabControlChart;

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		_wrapPanel = new WrapPanel()
		{
			Orientation = ListGroup.Horizontal ? Orientation.Horizontal : Orientation.Vertical,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			Margin = new Thickness(6),
		};

		_scrollViewer = new ScrollViewer()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			Content = _wrapPanel,
		};

		Children.Add(_scrollViewer);

		if (ListGroup.ShowLegend && ListGroup.ShowOrder && !ListGroup.Horizontal)
		{
			_textBlockTotal = new TextBlock()
			{
				Foreground = Theme.BackgroundText,
				Margin = new Thickness(2, 2, 2, 2),
				HorizontalAlignment = HorizontalAlignment.Right,
			};
			if (ListGroup.UnitName != null)
				_textBlockTotal.Text += " - " + ListGroup.UnitName;
		}

		RefreshModel();
	}

	private string GetTotalName()
	{
		var seriesType = SeriesType.Other;

		foreach (var series in ListGroup.Series)
		{
			if (seriesType == SeriesType.Other)
				seriesType = series.SeriesType;
			else if (series.SeriesType != seriesType)
				return "Total";
		}

		return seriesType.ToString();
	}

	private TabChartLegendItem AddSeries(OxyListSeries oxyListSeries)
	{
		OxyPlot.Series.Series series = oxyListSeries.OxySeries;

		Color color = Colors.Green;
		if (series is OxyPlot.Series.LineSeries lineSeries)
			color = lineSeries.Color.ToColor();
		if (series is OxyPlot.Series.ScatterSeries scatterSeries)
			color = scatterSeries.MarkerFill.ToColor();

		var legendItem = new TabChartLegendItem(this, oxyListSeries);
		legendItem.OnSelectionChanged += LegendItem_SelectionChanged;
		legendItem.OnVisibleChanged += LegendItem_VisibleChanged;
		legendItem.TextBlock!.PointerPressed += (s, e) =>
		{
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
				SelectLegendItem(legendItem);
		};
		LegendItems.Add(legendItem);
		if (series.Title != null)
			_idxLegendItems.Add(series.Title, legendItem);
		return legendItem;
	}

	// Show items in order of count, retaining original order for unused values
	private void UpdatePositions()
	{
		_wrapPanel.Children.Clear();
		if (_textBlockTotal != null)
			_wrapPanel.Children.Add(_textBlockTotal);

		var nonzero = new List<TabChartLegendItem>();
		var unused = new List<TabChartLegendItem>();
		foreach (TabChartLegendItem legendItem in _idxLegendItems.Values)
		{
			if (legendItem.Count > 0)
				nonzero.Add(legendItem);
			else
				unused.Add(legendItem);
		}

		var ordered = nonzero.OrderByDescending(a => a.Total).ToList();
		ordered.AddRange(unused);
		if (ListGroup.ShowLegend && ListGroup.ShowOrder && !ListGroup.Horizontal)
		{
			for (int i = 0; i < ordered.Count; i++)
				ordered[i].Index = i + 1;
		}
		_wrapPanel.Children.AddRange(ordered);
	}

	private void SelectLegendItem(TabChartLegendItem legendItem)
	{
		int selectedCount = 0;
		foreach (TabChartLegendItem item in LegendItems)
		{
			if (item.IsSelected)
				selectedCount++;
		}

		if (legendItem.IsSelected == false || selectedCount > 1)
		{
			SetAllVisible(false);
			legendItem.IsSelected = true;
			//OnSelectionChanged?.Invoke(this, legendItem.oxyListSeries);
		}
		else
		{
			SetAllVisible(true);
		}

		UpdateVisibleSeries();
		OnSelectionChanged?.Invoke(this, new());
		//if (legendItem.checkBox.IsChecked == true)
		//SetSelectionAll(legendItem.checkBox.IsChecked == true);
	}

	public void SelectSeries(OxyPlot.Series.Series oxySeries)
	{
		if (oxySeries.Title == null)
			return;

		if (_idxLegendItems.TryGetValue(oxySeries.Title, out TabChartLegendItem? legendItem))
		{
			SelectLegendItem(legendItem);
		}
	}

	public void HighlightSeries(OxyPlot.Series.Series oxySeries)
	{
		if (oxySeries.Title == null)
			return;

		// Clear all first before setting to avoid event race conditions
		foreach (TabChartLegendItem item in LegendItems)
			item.Highlight = false;

		if (_idxLegendItems.TryGetValue(oxySeries.Title, out TabChartLegendItem? legendItem))
		{
			foreach (TabChartLegendItem item in LegendItems)
				item.Highlight = (legendItem == item);
		}
		UpdateVisibleSeries();
	}

	public void SetAllVisible(bool selected, bool update = false)
	{
		bool changed = false;
		foreach (TabChartLegendItem legendItem in LegendItems)
		{
			changed |= (legendItem.IsSelected != selected);
			legendItem.IsSelected = selected;
		}

		if (update && changed)
		{
			UpdateVisibleSeries();
			OnSelectionChanged?.Invoke(this, new());
		}
	}

	public void RefreshModel()
	{
		if (PlotView!.Model == null)
			return;

		_wrapPanel.Children.Clear();
		foreach (var oxyListSeries in TabControlChart.OxyListSeriesList)
		{
			string title = oxyListSeries.OxySeries.Title;
			if (title == null)
				continue;

			if (!_idxLegendItems.TryGetValue(title, out TabChartLegendItem? legendItem))
			{
				legendItem = AddSeries(oxyListSeries);
			}
			else
			{
				legendItem.UpdateTotal();
			}

			if (!_wrapPanel.Children.Contains(legendItem))
				_wrapPanel.Children.Add(legendItem);
		}
		UpdatePositions();

		if (_textBlockTotal != null)
			_textBlockTotal.Text = GetTotalName();

		// Possibly faster? But more likely to cause problems
		/*var prevLegends = idxLegendItems.Clone<Dictionary<string, TabChartLegendItem>>();
		idxLegendItems = new Dictionary<string, TabChartLegendItem>();
		int row = 0;
		foreach (var series in plotView.Model.Series)
		{
			TabChartLegendItem legendItem;
			if (!prevLegends.TryGetValue(series.Title, out legendItem))
			{
				legendItem = AddSeries(series);
				prevLegends.Remove(series.Title);
			}
			idxLegendItems.Add(series.Title, legendItem);
			Grid.SetRow(legendItem, row++);
		}*/

		Dispatcher.UIThread.InvokeAsync(() => PlotView.Model?.InvalidatePlot(true), DispatcherPriority.Background);
	}

	public void Unload()
	{
		_wrapPanel.Children.Clear();
		_idxLegendItems.Clear();
		LegendItems.Clear();
	}

	public void UpdateVisibleSeries()
	{
		if (PlotView!.Model == null)
			return;

		foreach (OxyPlot.Series.Series series in PlotView.Model.Series)
		{
			if (series is OxyPlot.Series.LineSeries lineSeries)
			{
				if (lineSeries.Title == null)
					continue;

				if (_idxLegendItems.TryGetValue(lineSeries.Title, out TabChartLegendItem? legendItem))
				{
					legendItem.UpdateVisible(lineSeries);
				}
			}

			if (series is OxyPlot.Series.ScatterSeries scatterSeries)
			{
				if (scatterSeries.Title == null)
					continue;

				if (_idxLegendItems.TryGetValue(scatterSeries.Title, out TabChartLegendItem? legendItem))
				{
					legendItem.UpdateVisible(scatterSeries);
				}
			}
		}
		Dispatcher.UIThread.InvokeAsync(() => PlotView.Model?.InvalidatePlot(true), DispatcherPriority.Background);
	}

	private void LegendItem_SelectionChanged(object? sender, EventArgs e)
	{
		UpdateVisibleSeries();
		OnSelectionChanged?.Invoke(this, new());
	}

	private void LegendItem_VisibleChanged(object? sender, EventArgs e)
	{
		UpdateVisibleSeries();
		OnVisibleChanged?.Invoke(this, new());
	}

	public void UnhighlightAll(bool update = false)
	{
		foreach (TabChartLegendItem item in LegendItems)
		{
			item.Highlight = false;
		}

		if (update)
			UpdateVisibleSeries();
	}

	public void UpdateHighlight(bool showFaded)
	{
		foreach (TabChartLegendItem item in LegendItems)
		{
			item.UpdateHighlight(showFaded);
		}
	}
}
