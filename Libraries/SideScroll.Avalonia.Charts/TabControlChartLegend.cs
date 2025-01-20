using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using SideScroll.Avalonia.Controls;
using SideScroll.Charts;
using SideScroll.Collections;

namespace SideScroll.Avalonia.Charts;

public abstract class TabControlChartLegend<TSeries> : Grid
{
	public TabControlChart<TSeries> TabControlChart { get; init; }
	public ChartView ChartView => TabControlChart.ChartView;

	public List<TabChartLegendItem<TSeries>> LegendItems { get; protected set; } = [];
	public Dictionary<string, TabChartLegendItem<TSeries>> IdxLegendItems { get; protected set; } = [];

	protected ScrollViewer ScrollViewer { get; set; }
	protected WrapPanel WrapPanel { get; set; }
	protected TextBlock? TextBlockTotal { get; set; }

	public event EventHandler<EventArgs>? OnSelectionChanged;
	public event EventHandler<EventArgs>? OnVisibleChanged;

	public override string? ToString() => ChartView.ToString();

	protected TabControlChartLegend(TabControlChart<TSeries> tabControlChart)
	{
		TabControlChart = tabControlChart;

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		WrapPanel = new WrapPanel
		{
			Orientation = ChartView.LegendPosition == ChartLegendPosition.Right ? Orientation.Vertical : Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			Margin = new Thickness(6),
		};

		ScrollViewer = new ScrollViewer
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			Content = WrapPanel,
		};

		Children.Add(ScrollViewer);

		if (ChartView.ShowOrder && ChartView.LegendPosition == ChartLegendPosition.Right)
		{
			TextBlockTotal = new TabControlTextBlock
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
		WrapPanel.Children.Clear();
		if (TextBlockTotal != null)
		{
			WrapPanel.Children.Add(TextBlockTotal);
		}

		var nonzero = new List<TabChartLegendItem<TSeries>>();
		var unused = new List<TabChartLegendItem<TSeries>>();
		foreach (TabChartLegendItem<TSeries> legendItem in IdxLegendItems.Values)
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
		WrapPanel.Children.AddRange(ordered);
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

		if (IdxLegendItems.TryGetValue(listSeries.Name, out TabChartLegendItem<TSeries>? legendItem))
		{
			SelectLegendItem(legendItem);
		}
	}

	public void HighlightSeries(string? name)
	{
		if (name == null) return;

		if (IdxLegendItems.TryGetValue(name, out TabChartLegendItem<TSeries>? legendItem))
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
		WrapPanel.Children.Clear();
		foreach (ChartSeries<TSeries> chartSeries in TabControlChart.ChartSeries)
		{
			string? title = chartSeries.ToString();
			if (title == null) continue;

			if (!IdxLegendItems.TryGetValue(title, out TabChartLegendItem<TSeries>? legendItem))
			{
				legendItem = AddSeries(chartSeries);
			}
			else
			{
				legendItem.UpdateTotal();
			}

			if (!WrapPanel.Children.Contains(legendItem))
			{
				WrapPanel.Children.Add(legendItem);
			}
		}
		UpdatePositions();

		if (TextBlockTotal != null)
		{
			TextBlockTotal.Text = ChartView.LegendTitle ?? GetTotalName();
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
		WrapPanel.Children.Clear();
		IdxLegendItems.Clear();
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

