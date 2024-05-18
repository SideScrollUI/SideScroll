using Atlas.Core;
using Atlas.Core.Charts;
using Atlas.UI.Avalonia.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Atlas.UI.Avalonia.Charts;

public abstract class TabControlChartLegend<TSeries> : Grid
{
	public TabControlChart<TSeries> TabControlChart;
	public ChartView ChartView => TabControlChart.ChartView;

	public List<TabChartLegendItem<TSeries>> LegendItems = [];
	protected readonly Dictionary<string, TabChartLegendItem<TSeries>> _idxLegendItems = [];

	protected readonly ScrollViewer _scrollViewer;
	protected readonly WrapPanel _wrapPanel;
	protected readonly TextBlock? _textBlockTotal;

	public event EventHandler<EventArgs>? OnSelectionChanged;
	public event EventHandler<EventArgs>? OnVisibleChanged;

	public override string? ToString() => ChartView.ToString();

	protected TabControlChartLegend(TabControlChart<TSeries> tabControlChart)
	{
		TabControlChart = tabControlChart;

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		_wrapPanel = new WrapPanel
		{
			Orientation = ChartView.LegendPosition == ChartLegendPosition.Right ? Orientation.Vertical : Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			Margin = new Thickness(6),
		};

		_scrollViewer = new ScrollViewer
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			Content = _wrapPanel,
		};

		Children.Add(_scrollViewer);

		if (ChartView.ShowOrder && ChartView.LegendPosition == ChartLegendPosition.Right)
		{
			_textBlockTotal = new TabControlTextBlock
			{
				Margin = new Thickness(2),
				HorizontalAlignment = HorizontalAlignment.Right,
			};
		}

		RefreshModel();
	}

	private string GetTotalName()
	{
		var seriesType = SeriesType.Other;

		foreach (var series in ChartView.Series)
		{
			if (seriesType == SeriesType.Other)
			{
				seriesType = series.SeriesType;
			}
			else if (series.SeriesType != seriesType)
			{
				return "Total";
			}
		}

		return seriesType.ToString();
	}

	protected abstract TabChartLegendItem<TSeries> AddSeries(ChartSeries<TSeries> chartSeries);

	// Show items in order of count, retaining original order for unused values
	private void UpdatePositions()
	{
		_wrapPanel.Children.Clear();
		if (_textBlockTotal != null)
		{
			_wrapPanel.Children.Add(_textBlockTotal);
		}

		var nonzero = new List<TabChartLegendItem<TSeries>>();
		var unused = new List<TabChartLegendItem<TSeries>>();
		foreach (TabChartLegendItem<TSeries> legendItem in _idxLegendItems.Values)
		{
			if (legendItem.Count > 0)
				nonzero.Add(legendItem);
			else
				unused.Add(legendItem);
		}

		var ordered = nonzero.OrderByDescending(a => a.Total).ToList();
		ordered.AddRange(unused);
		if (ChartView.ShowOrder && ChartView.LegendPosition == ChartLegendPosition.Right)
		{
			for (int i = 0; i < ordered.Count; i++)
			{
				ordered[i].Index = i + 1;
			}
		}
		_wrapPanel.Children.AddRange(ordered);
	}

	protected void SelectLegendItem(TabChartLegendItem<TSeries> legendItem)
	{
		int selectedCount = LegendItems.Count(item => item.IsSelected);

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
		OnSelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	public void SelectSeries(TSeries series, ListSeries listSeries)
	{
		if (listSeries.Name == null) return;

		if (_idxLegendItems.TryGetValue(listSeries.Name, out TabChartLegendItem<TSeries>? legendItem))
		{
			SelectLegendItem(legendItem);
		}
	}

	public void HighlightSeries(string? name)
	{
		if (name == null) return;

		if (_idxLegendItems.TryGetValue(name, out TabChartLegendItem<TSeries>? legendItem))
		{
			// Clear all first before setting to avoid event race conditions
			foreach (TabChartLegendItem<TSeries> item in LegendItems)
			{
				if (legendItem != item)
				{
					item.Highlight = false;
				}
			}

			foreach (TabChartLegendItem<TSeries> item in LegendItems)
			{
				if (legendItem == item)
				{
					item.Highlight = true;
				}
			}
		}
		UpdateVisibleSeries();
	}

	public void SetAllVisible(bool selected, bool update = false)
	{
		bool changed = false;
		foreach (TabChartLegendItem<TSeries> legendItem in LegendItems)
		{
			changed |= (legendItem.IsSelected != selected);
			legendItem.IsSelected = selected;
		}

		if (update && changed)
		{
			UpdateVisibleSeries();
			OnSelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public void RefreshModel()
	{
		_wrapPanel.Children.Clear();
		foreach (ChartSeries<TSeries> chartSeries in TabControlChart.ChartSeries)
		{
			string? title = chartSeries.ToString();
			if (title == null) continue;

			if (!_idxLegendItems.TryGetValue(title, out TabChartLegendItem<TSeries>? legendItem))
			{
				legendItem = AddSeries(chartSeries);
			}
			else
			{
				legendItem.UpdateTotal();
			}

			if (!_wrapPanel.Children.Contains(legendItem))
			{
				_wrapPanel.Children.Add(legendItem);
			}
		}
		UpdatePositions();

		if (_textBlockTotal != null)
		{
			_textBlockTotal.Text = ChartView.LegendTitle ?? GetTotalName();
		}

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
	}

	public void Unload()
	{
		_wrapPanel.Children.Clear();
		_idxLegendItems.Clear();
		LegendItems.Clear();
	}

	public virtual void UpdateVisibleSeries()
	{
		if (TabControlChart == null) return;

		foreach (TabChartLegendItem<TSeries> legendItem in LegendItems)
		{
			legendItem.UpdateVisible();
		}
	}

	// todo: remove 1
	protected void LegendItem_SelectionChanged(object? sender, EventArgs e)
	{
		UpdateVisibleSeries();
		OnSelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	protected void LegendItem_VisibleChanged(object? sender, EventArgs e)
	{
		UpdateVisibleSeries();
		OnVisibleChanged?.Invoke(this, EventArgs.Empty);
	}

	public virtual void UpdateHighlight(bool showFaded)
	{
		foreach (TabChartLegendItem<TSeries> item in LegendItems)
		{
			item.UpdateHighlight(showFaded);
		}
	}

	public void UnhighlightAll(bool update = false)
	{
		foreach (TabChartLegendItem<TSeries> item in LegendItems)
		{
			item.Highlight = false;
		}

		if (update)
		{
			UpdateVisibleSeries();
		}
	}
}

