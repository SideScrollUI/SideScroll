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

/// <summary>
/// Abstract base class for the chart legend panel. Manages a scrollable, wrapping list of
/// <see cref="TabChartLegendItem{TSeries}"/> controls and handles series selection, highlighting, and clipboard export.
/// </summary>
public abstract class TabChartLegend<TSeries> : Grid
{
	/// <summary>Gets the parent chart control this legend belongs to.</summary>
	public TabChart<TSeries> TabChart { get; }
	/// <summary>Gets the chart view data model.</summary>
	public ChartView ChartView => TabChart.ChartView;

	/// <summary>Gets the ordered list of legend items.</summary>
	public List<TabChartLegendItem<TSeries>> LegendItems { get; } = [];
	/// <summary>Gets the index of legend items keyed by series name.</summary>
	public Dictionary<string, TabChartLegendItem<TSeries>> IdxLegendItems { get; } = [];

	/// <summary>Gets the scroll viewer that wraps the legend items panel.</summary>
	protected ScrollViewer ScrollViewer { get; }
	/// <summary>Gets the wrap panel that lays out legend items horizontally or vertically.</summary>
	protected WrapPanel WrapPanel { get; }
	/// <summary>Gets the optional text block showing the aggregate column header (e.g., "Total" or "Count").</summary>
	protected TextBlock? TextBlockTotal { get; }

	/// <summary>Raised when the visible set of series changes due to selection or highlight actions.</summary>
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

	/// <summary>Creates and registers a legend item for the given chart series.</summary>
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

	/// <summary>Selects a single legend item, deselecting all others, or restores full visibility if it was already the only selected item.</summary>
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

	/// <summary>Selects the legend item corresponding to the given series by name.</summary>
	public void SelectSeries(TSeries series, ListSeries listSeries)
	{
		if (listSeries.Name == null) return;

		if (IdxLegendItems.TryGetValue(listSeries.Name, out TabChartLegendItem<TSeries>? legendItem))
		{
			SelectLegendItem(legendItem);
		}
	}

	/// <summary>Highlights the legend item with the given series name, fading all others.</summary>
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

	/// <summary>Sets all legend items to the specified selection state, optionally triggering a visible-series update.</summary>
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

	/// <summary>Synchronizes the legend items with the current chart series, adding new items and updating totals for existing ones.</summary>
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

	/// <summary>Clears all legend items and resets state.</summary>
	public void Unload()
	{
		WrapPanel.Children.Clear();
		IdxLegendItems.Clear();
		LegendItems.Clear();
	}

	/// <summary>Updates the visible state of all legend items based on their current selection and highlight.</summary>
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

	/// <summary>Updates each legend item's color to either its full color or a faded version, based on highlight state.</summary>
	public virtual void UpdateHighlight(bool showFaded)
	{
		foreach (TabChartLegendItem<TSeries> item in LegendItems)
		{
			item.UpdateHighlight(showFaded);
		}
	}

	/// <summary>Clears the highlight from all legend items, optionally refreshing series visibility.</summary>
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

