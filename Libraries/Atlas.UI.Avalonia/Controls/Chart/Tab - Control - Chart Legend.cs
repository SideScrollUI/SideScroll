using Atlas.Core;
using Avalonia;
using Avalonia.Controls;
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
	public class TabControlChartLegend : WrapPanel
	{
		private TabControlChart tabControlChart;
		private PlotView plotView;
		public ListGroup listGroup;
		private TextBlock textBlockSum;
		public List<TabChartLegendItem> legendItems = new List<TabChartLegendItem>();
		public Dictionary<string, TabChartLegendItem> idxLegendItems = new Dictionary<string, TabChartLegendItem>();

		public bool IsHorizontal { get; set; }

		public event EventHandler<EventArgs> OnSelectionChanged;
		public event EventHandler<EventArgs> OnVisibleChanged;

		public TabControlChartLegend(TabControlChart tabControlChart, bool horizontal)
		{
			this.tabControlChart = tabControlChart;
			this.plotView = tabControlChart.plotView;
			this.listGroup = tabControlChart.ListGroup;
			this.IsHorizontal = horizontal;
			InitializeControls();
		}

		private void InitializeControls()
		{
			HorizontalAlignment = HorizontalAlignment.Left;
			//VerticalAlignment = VerticalAlignment.Stretch;
			Margin = new Thickness(6);
			Orientation = IsHorizontal ? Orientation.Horizontal : Orientation.Vertical;

			if (listGroup.ShowOrder && !IsHorizontal)
			{
				textBlockSum = new TextBlock()
				{
					Text = "Total",
					Foreground = Brushes.LightGray,
					Margin = new Thickness(2, 2, 2, 2),
					HorizontalAlignment = HorizontalAlignment.Right,
				};
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
					LegendItemClicked(legendItem);
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
			Children.Clear();
			if (textBlockSum != null)
				Children.Add(textBlockSum);

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
			if (listGroup.ShowOrder)
			{
				for (int i = 0; i < ordered.Count; i++)
					ordered[i].Index = i + 1;
			}
			Children.AddRange(ordered);
		}

		private void LegendItemClicked(TabChartLegendItem legendItem)
		{
			int selectedCount = 0;
			foreach (TabChartLegendItem item in legendItems)
			{
				if (item.IsChecked == true)
					selectedCount++;
			}
			if (legendItem.IsChecked == false || selectedCount > 1)
			{
				SetSelectionAll(false);
				legendItem.IsChecked = true;
				//OnSelectionChanged?.Invoke(this, legendItem.oxyListSeries);
			}
			else
			{
				SetSelectionAll(true);
			}
			UpdateVisibleSeries();
			OnSelectionChanged?.Invoke(this, null);
			//if (legendItem.checkBox.IsChecked == true)
			//SetSelectionAll(legendItem.checkBox.IsChecked == true);
		}

		private void SetSelectionAll(bool selected)
		{
			foreach (TabChartLegendItem legendItem in legendItems)
			{
				legendItem.IsChecked = selected;
			}
		}

		public void RefreshModel()
		{
			if (plotView.Model == null)
				return;

			Children.Clear();
			foreach (var oxyListSeries in tabControlChart.oxyListSeriesList)
			{
				string title = oxyListSeries.OxySeries.Title;
				if (title == null)
					continue;
				if (!idxLegendItems.TryGetValue(title, out TabChartLegendItem legendItem))
				{
					legendItem = AddSeries(oxyListSeries);
				}
				if (!Children.Contains(legendItem))
					Children.Add(legendItem);
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
			Children.Clear();
			idxLegendItems.Clear();
			legendItems.Clear();
		}

		private void UpdateVisibleSeries()
		{
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

		public void HighlightAll(bool showFaded)
		{
			foreach (TabChartLegendItem item in legendItems)
			{
				item.UpdateHighlight(showFaded);
			}
		}
	}
}
