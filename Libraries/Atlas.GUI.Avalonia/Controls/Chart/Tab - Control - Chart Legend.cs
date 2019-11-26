using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;

using OxyPlot;
using OxyPlot.Avalonia;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlChartLegend : Grid
	{
		private PlotView plotView;

		public event EventHandler<EventArgs> OnSelectionChanged;

		public TabControlChartLegend(PlotView plotView)
		{
			this.plotView = plotView;
			InitializeControls();
		}

		private void InitializeControls()
		{
			//this.HorizontalAlignment = HorizontalAlignment.Right;
			//this.VerticalAlignment = VerticalAlignment.Stretch;
			this.ColumnDefinitions = new ColumnDefinitions("Auto");
			this.RowDefinitions = new RowDefinitions();
			//this.Margin = new Thickness(6);

			RefreshModel();
		}
		public List<TabChartLegendItem> legendItems = new List<TabChartLegendItem>();
		public Dictionary<string, TabChartLegendItem> idxLegendItems = new Dictionary<string, TabChartLegendItem>();

		private TabChartLegendItem AddSeries(OxyPlot.Series.Series series)
		{
			RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			Color color = Colors.Green;
			if (series is OxyPlot.Series.LineSeries lineSeries)
				color = lineSeries.Color.ToColor();
			TabChartLegendItem legendItem = new TabChartLegendItem(series)
			{
				[Grid.RowProperty] = Children.Count,
			};
			//legendItem.PointerEnter += CheckBox_PointerEnter;
			//legendItem.PointerLeave += CheckBox_PointerLeave;
			legendItem.OnSelectionChanged += CheckBox_SelectionChanged;
			legendItem.textBlock.PointerPressed += (s, e) =>
			{
				LegendItemClicked(legendItem);
			};
			this.Children.Add(legendItem);
			legendItems.Add(legendItem);
			idxLegendItems.Add(series.Title, legendItem);
			return legendItem;
		}

		private void UpdateSeriesPositions()
		{
		}

		private void LegendItemClicked(TabChartLegendItem legendItem)
		{
			int selectedCount = 0;
			foreach (TabChartLegendItem item in legendItems)
			{
				if (item.checkBox.IsChecked == true)
					selectedCount++;
			}
			if (selectedCount > 1)
			{
				SetSelectionAll(false);
				legendItem.checkBox.IsChecked = true;
			}
			else
			{
				SetSelectionAll(true);
			}
			UpdateSelected();
			//if (legendItem.checkBox.IsChecked == true)
			//SetSelectionAll(legendItem.checkBox.IsChecked == true);
		}

		private void SetSelectionAll(bool selected)
		{
			foreach (TabChartLegendItem legendItem in legendItems)
			{
				legendItem.checkBox.IsChecked = selected;
			}
		}

		public void RefreshModel()
		{
			if (plotView.Model != null)
			{
				int row = 0;
				foreach (var series in plotView.Model.Series)
				{
					TabChartLegendItem legendItem;
					if (!idxLegendItems.TryGetValue(series.Title, out legendItem))
						legendItem = AddSeries(series);
					Grid.SetRow(legendItem, row++);
				}
			}
		}

		private void UpdateSelected()
		{
			/*foreach (CheckBox checkBox in checkBoxes)
			{
				if (checkBox.DataContext is OxyPlot.Series.LineSeries lineSeries)
				{
					if (checkBox.IsChecked == true)
					{
						lineSeries.LineStyle = LineStyle.Solid;
						lineSeries.MarkerType = MarkerType.Circle;
					}
					else
					{
						lineSeries.LineStyle = LineStyle.None;
						lineSeries.MarkerType = MarkerType.None;
						lineSeries.Unselect();
					}
				}
			}*/
			foreach (OxyPlot.Series.Series series in plotView.Model.Series)
			{
				if (series is OxyPlot.Series.LineSeries lineSeries)
				{
					TabChartLegendItem legendItem;
					if (idxLegendItems.TryGetValue(series.Title, out legendItem))
					{
						legendItem.series = series;
						if (legendItem.checkBox.IsChecked == true)
						{
							lineSeries.LineStyle = LineStyle.Solid;
							lineSeries.MarkerType = MarkerType.Circle;
						}
						else
						{
							lineSeries.LineStyle = LineStyle.None;
							lineSeries.MarkerType = MarkerType.None;
							lineSeries.Unselect();
						}
					}
				}
			}
			OnSelectionChanged?.Invoke(this, null);
		}

		private void HighlightSeries(double thickness, double markerSize)
		{
			foreach (OxyPlot.Series.Series series in plotView.Model.Series)
			{
				if (series is OxyPlot.Series.LineSeries lineSeries)
				{
					lineSeries.MarkerSize = markerSize;
					lineSeries.StrokeThickness = thickness;
				}
			}
		}

		private void CheckBox_SelectionChanged(object sender, EventArgs e)
		{
			UpdateSelected();
			Dispatcher.UIThread.InvokeAsync(() => plotView.Model.InvalidatePlot(true), DispatcherPriority.Background);
		}

		private void CheckBox_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			HighlightSeries(2, 3);
			OnSelectionChanged?.Invoke(this, null);
			Dispatcher.UIThread.InvokeAsync(() => plotView.Model.InvalidatePlot(true), DispatcherPriority.Background);
		}

		private void CheckBox_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			TabChartLegendItem legendItem = (TabChartLegendItem)sender;
			HighlightSeries(1, 2);
			var lineSeries = (OxyPlot.Series.LineSeries)legendItem.series;
			lineSeries.StrokeThickness = 2;
			lineSeries.MarkerSize = 3;
			OnSelectionChanged?.Invoke(this, null);
			Dispatcher.UIThread.InvokeAsync(() => plotView.Model.InvalidatePlot(true), DispatcherPriority.Background);
		}
	}
}
