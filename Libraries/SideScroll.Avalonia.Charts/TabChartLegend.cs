using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Utilities;
using SideScroll.Charts;
using SideScroll.Collections;

namespace SideScroll.Avalonia.Charts;

public abstract class TabChartLegend<TSeries> : Grid
{
	public TabChart<TSeries> TabChart { get; }
	public ChartView ChartView => TabChart.ChartView;

	public List<TabChartLegendItem<TSeries>> LegendItems { get; } = [];
	public Dictionary<string, TabChartLegendItem<TSeries>> IdxLegendItems { get; } = [];

	protected ScrollViewer ScrollViewer { get; }
	protected WrapPanel WrapPanel { get; }
	protected TextBlock? TextBlockTotal { get; }

	public event EventHandler<EventArgs>? OnVisibleSeriesChanged;

	public override string? ToString() => ChartView.ToString();

	protected TabChartLegend(TabChart<TSeries> tabChart)
	{
		TabChart = tabChart;

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
			TextBlockTotal = new TabTextBlock
			{
				Margin = new Thickness(2),
				HorizontalAlignment = HorizontalAlignment.Right,
			};
		}

		AddContextMenu();

		RefreshModel();
	}

	private void AddContextMenu()
	{
		var menuItemCopyAll = new TabMenuItem("Copy - _All");
		menuItemCopyAll.Click += async delegate
		{
			await CopyToClipboardAsync(false);
		};

		var menuItemCopySelected = new TabMenuItem("Copy - _Selected");
		menuItemCopySelected.Click += async delegate
		{
			await CopyToClipboardAsync(true);
		};

		ContextMenu = new ContextMenu
		{
			ItemsSource = new AvaloniaList<object>
			{
				menuItemCopyAll,
				menuItemCopySelected,
			}
		};
	}

	private async Task CopyToClipboardAsync(bool selectedOnly)
	{
		List<TableUtils.ColumnInfo> columns =
		[
			new("Name"),
			new("Total") { RightAlign = TextAlignment.Right }
		];

		List<List<string>> contentRows = [];
		foreach (TabChartLegendItem<TSeries> legendItem in LegendItems)
		{
			if (selectedOnly && !legendItem.IsSelected)
				continue;

			List<string> row =
			[
				legendItem.ToString() ?? "",
				legendItem.Total?.ToString() ?? ""
			];
			contentRows.Add(row);
		}

		string tableText = TableUtils.TableToString(columns, contentRows);
		await ClipboardUtils.SetTextAsync(this, tableText);
	}

	private string GetTotalName()
	{
		var seriesType = SeriesType.Other;

		foreach (ListSeries series in ChartView.Series)
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
			{
				nonzero.Add(legendItem);
			}
			else
			{
				unused.Add(legendItem);
			}
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
		}
		else
		{
			SetAllVisible(true);
		}

		UpdateVisibleSeries();
		OnVisibleSeriesChanged?.Invoke(this, EventArgs.Empty);
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
			OnVisibleSeriesChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public void RefreshModel()
	{
		WrapPanel.Children.Clear();
		foreach (ChartSeries<TSeries> chartSeries in TabChart.ChartSeries)
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
		foreach (TabChartLegendItem<TSeries> legendItem in LegendItems)
		{
			legendItem.UpdateVisible();
		}
	}

	protected void LegendItem_VisibilityChanged(object? sender, EventArgs e)
	{
		UpdateVisibleSeries();
		OnVisibleSeriesChanged?.Invoke(this, EventArgs.Empty);
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

