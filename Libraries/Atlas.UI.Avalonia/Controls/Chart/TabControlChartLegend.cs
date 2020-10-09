﻿using Atlas.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using OxyPlot.Avalonia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlChartLegend : Grid
	{
		public TabControlChart TabControlChart;
		private PlotView plotView;
		public ListGroup ListGroup;
		private ScrollViewer scrollViewer;
		private WrapPanel wrapPanel;
		private TextBlock textBlockSum;
		public List<TabChartLegendItem> legendItems = new List<TabChartLegendItem>();
		public Dictionary<string, TabChartLegendItem> idxLegendItems = new Dictionary<string, TabChartLegendItem>();

		public event EventHandler<EventArgs> OnSelectionChanged;
		public event EventHandler<EventArgs> OnVisibleChanged;

		public TabControlChartLegend(TabControlChart tabControlChart)
		{
			TabControlChart = tabControlChart;
			plotView = tabControlChart.PlotView;
			ListGroup = tabControlChart.ListGroup;
			InitializeControls();
		}

		private void InitializeControls()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;

			wrapPanel = new WrapPanel()
			{
				Orientation = ListGroup.Horizontal ? Orientation.Horizontal : Orientation.Vertical,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Margin = new Thickness(6),
			};

			scrollViewer = new ScrollViewer()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				Content = wrapPanel,
			};

			Children.Add(scrollViewer);

			if (ListGroup.ShowLegend && ListGroup.ShowOrder && !ListGroup.Horizontal)
			{
				textBlockSum = new TextBlock()
				{
					Text = "Total",
					Foreground = Theme.BackgroundText,
					Margin = new Thickness(2, 2, 2, 2),
					HorizontalAlignment = HorizontalAlignment.Right,
				};
				if (ListGroup.UnitName != null)
					textBlockSum.Text += " - " + ListGroup.UnitName;
			}

			RefreshModel();
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
			legendItem.textBlock.PointerPressed += (s, e) =>
			{
				if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
					SelectLegendItem(legendItem);
			};
			//this.Children.Add(legendItem);
			legendItems.Add(legendItem);
			if (series.Title != null)
				idxLegendItems.Add(series.Title, legendItem);
			return legendItem;
		}

		// Show items in order of count, retaining original order for unused values
		private void UpdatePositions()
		{
			wrapPanel.Children.Clear();
			if (textBlockSum != null)
				wrapPanel.Children.Add(textBlockSum);

			var nonzero = new List<TabChartLegendItem>();
			var unused = new List<TabChartLegendItem>();
			foreach (TabChartLegendItem legendItem in idxLegendItems.Values)
			{
				if (legendItem.Count > 0)
					nonzero.Add(legendItem);
				else
					unused.Add(legendItem);
			}

			var ordered = nonzero.OrderByDescending(a => a.Sum).ToList();
			ordered.AddRange(unused);
			if (ListGroup.ShowLegend && ListGroup.ShowOrder && !ListGroup.Horizontal)
			{
				for (int i = 0; i < ordered.Count; i++)
					ordered[i].Index = i + 1;
			}
			wrapPanel.Children.AddRange(ordered);
		}

		private void SelectLegendItem(TabChartLegendItem legendItem)
		{
			int selectedCount = 0;
			foreach (TabChartLegendItem item in legendItems)
			{
				if (item.IsChecked == true)
					selectedCount++;
			}
			if (legendItem.IsChecked == false || selectedCount > 1)
			{
				SetAllVisible(false);
				legendItem.IsChecked = true;
				//OnSelectionChanged?.Invoke(this, legendItem.oxyListSeries);
			}
			else
			{
				SetAllVisible(true);
			}
			UpdateVisibleSeries();
			OnSelectionChanged?.Invoke(this, null);
			//if (legendItem.checkBox.IsChecked == true)
			//SetSelectionAll(legendItem.checkBox.IsChecked == true);
		}

		public void SelectSeries(OxyPlot.Series.Series oxySeries)
		{
			if (oxySeries.Title == null)
				return;
			if (idxLegendItems.TryGetValue(oxySeries.Title, out TabChartLegendItem legendItem))
			{
				SelectLegendItem(legendItem);
			}
		}

		public void HighlightSeries(OxyPlot.Series.Series oxySeries)
		{
			if (oxySeries.Title == null)
				return;
			// Clear all first before setting to avoid event race conditions
			foreach (TabChartLegendItem item in legendItems)
				item.Highlight = false;
			if (idxLegendItems.TryGetValue(oxySeries.Title, out TabChartLegendItem legendItem))
			{
				foreach (TabChartLegendItem item in legendItems)
					item.Highlight = (legendItem == item);
			}
			UpdateVisibleSeries();
		}

		public void SetAllVisible(bool selected, bool update = false)
		{
			foreach (TabChartLegendItem legendItem in legendItems)
			{
				legendItem.IsChecked = selected;
			}
			if (update)
			{
				UpdateVisibleSeries();
				OnSelectionChanged?.Invoke(this, null);
			}
		}

		public void RefreshModel()
		{
			if (plotView.Model == null)
				return;

			wrapPanel.Children.Clear();
			foreach (var oxyListSeries in TabControlChart.OxyListSeriesList)
			{
				string title = oxyListSeries.OxySeries.Title;
				if (title == null)
					continue;
				if (!idxLegendItems.TryGetValue(title, out TabChartLegendItem legendItem))
				{
					legendItem = AddSeries(oxyListSeries);
				}
				if (!wrapPanel.Children.Contains(legendItem))
					wrapPanel.Children.Add(legendItem);
			}
			UpdatePositions();

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

			Dispatcher.UIThread.InvokeAsync(() => plotView.Model?.InvalidatePlot(true), DispatcherPriority.Background);
		}

		public void Unload()
		{
			wrapPanel.Children.Clear();
			idxLegendItems.Clear();
			legendItems.Clear();
		}

		public void UpdateVisibleSeries()
		{
			if (plotView.Model == null)
				return;
			foreach (OxyPlot.Series.Series series in plotView.Model.Series)
			{
				if (series is OxyPlot.Series.LineSeries lineSeries)
				{
					if (lineSeries.Title == null)
						continue;
					if (idxLegendItems.TryGetValue(lineSeries.Title, out TabChartLegendItem legendItem))
					{
						legendItem.UpdateVisible(lineSeries);
					}
				}
				if (series is OxyPlot.Series.ScatterSeries scatterSeries)
				{
					if (scatterSeries.Title == null)
						continue;
					if (idxLegendItems.TryGetValue(scatterSeries.Title, out TabChartLegendItem legendItem))
					{
						legendItem.UpdateVisible(scatterSeries);
					}
				}
			}
			Dispatcher.UIThread.InvokeAsync(() => plotView.Model.InvalidatePlot(true), DispatcherPriority.Background);
		}

		private void LegendItem_SelectionChanged(object sender, EventArgs e)
		{
			UpdateVisibleSeries();
			OnSelectionChanged?.Invoke(this, null);
		}

		private void LegendItem_VisibleChanged(object sender, EventArgs e)
		{
			UpdateVisibleSeries();
			OnVisibleChanged?.Invoke(this, null);
		}

		public void UnhighlightAll(bool update = false)
		{
			foreach (TabChartLegendItem item in legendItems)
			{
				item.Highlight = false;
			}
			if (update)
				UpdateVisibleSeries();
		}

		public void UpdateHighlight(bool showFaded)
		{
			foreach (TabChartLegendItem item in legendItems)
			{
				item.UpdateHighlight(showFaded);
			}
		}
	}
}