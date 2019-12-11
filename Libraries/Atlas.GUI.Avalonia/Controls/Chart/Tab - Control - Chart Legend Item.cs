using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections;

using OxyPlot.Avalonia;
using OxyPlot.Series;
using OxyPlot;
using Avalonia.Media.Imaging;
using Avalonia.Controls.Shapes;
using System.Collections.Generic;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabChartLegendItem : Grid
	{
		public event EventHandler<EventArgs> OnSelectionChanged;
		public event EventHandler<EventArgs> OnHighlightChanged;

		public OxyPlot.Series.Series series;
		//public string Label { get; set; }
		public TextBlock textBlock;
		private Polygon polygon;
		private Color color = Colors.Green;

		private bool _IsChecked = true;
		public bool IsChecked
		{
			get
			{
				return _IsChecked;
			}
			set
			{
				_IsChecked = value;
				polygon.Fill = new SolidColorBrush(IsChecked ? color : Colors.Transparent);
			}
		}

		public IEnumerable ItemsSource { get; internal set; }

		public TabChartLegendItem(OxyPlot.Series.Series series)
		{
			this.series = series;
			InitializeControls();
		}

		private void InitializeControls()
		{
			//this.HorizontalAlignment = HorizontalAlignment.Right;
			//this.VerticalAlignment = VerticalAlignment.Stretch;
			this.ColumnDefinitions = new ColumnDefinitions("Auto, Auto");
			this.RowDefinitions = new RowDefinitions();
			//this.Margin = new Thickness(6);

			AddRoundedCheckBox();
			AddTextBox();
		}

		private void AddRoundedCheckBox()
		{
			RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			
			if (series is OxyPlot.Series.LineSeries lineSeries)
				color = lineSeries.Color.ToColor();
			if (series is OxyPlot.Series.ScatterSeries scatterSeries)
				color = scatterSeries.MarkerFill.ToColor();

			int width = 13;
			int height = 13;
			polygon = new Polygon()
			{
				Width = 16,
				Height = 16,
				Fill = new SolidColorBrush(color),
				Stroke = Brushes.Black,
				StrokeThickness = 1.5,
			};
			UpdatePoints(width, height);
			polygon.PointerPressed += Polygon_PointerPressed;
			polygon.PointerEnter += Polygon_PointerEnter;
			polygon.PointerLeave += Polygon_PointerLeave;
			this.Children.Add(polygon);
		}

		private void UpdatePoints(int width, int height)
		{
			int cornerSize = 3;
			polygon.Points = new List<Point>()
			{
				new Point(0, height),
				new Point(width - cornerSize, height),
				new Point(width, height - cornerSize),
				new Point(width, 0),
				new Point(cornerSize, 0),
				new Point(0, cornerSize),
			};
		}

		private void Polygon_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e)
		{
			IsChecked = !IsChecked;
			OnSelectionChanged?.Invoke(this, null);
		}

		double? markerSize;
		private void Polygon_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			UpdatePoints(15, 15);
			if (series is OxyPlot.Series.LineSeries lineSeries)
			{
				markerSize = markerSize ?? lineSeries.MarkerSize;
				lineSeries.StrokeThickness = 4;
				lineSeries.MarkerSize = markerSize.Value + 2;
				OnHighlightChanged?.Invoke(this, null);
			}
			textBlock.Foreground = new SolidColorBrush(Theme.ActiveSelectionHighlightColor);
			//polygon.Stroke = new SolidColorBrush(Theme.GridColumnHeaderBackgroundColor);
			//polygon.Stroke = Brushes.White;
			//polygon.StrokeThickness = 2;
		}

		private void Polygon_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			UpdatePoints(13, 13);
			if (series is OxyPlot.Series.LineSeries lineSeries)
			{
				markerSize = markerSize ?? lineSeries.MarkerSize;
				lineSeries.StrokeThickness = 2;
				lineSeries.MarkerSize = markerSize.Value;
				//lineSeries.MarkerSize = 3; // store original?
				OnHighlightChanged?.Invoke(this, null);
			}
			textBlock.Foreground = Brushes.LightGray;
			//polygon.StrokeThickness = 4;
			//polygon.Stroke = Brushes.Black;
		}

		private void AddTextBox()
		{
			RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			textBlock = new TextBlock()
			{
				Text = series.Title,
				Foreground = Brushes.LightGray,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
				//BorderThickness = new Thickness(1),
				//BorderBrush = new SolidColorBrush(Colors.Black),
				Margin = new Thickness(2, 2, 6, 2),
				//VerticalAlignment = VerticalAlignment.Center,
				//HorizontalAlignment = HorizontalAlignment.Stretch,
				[Grid.ColumnProperty] = 1,
			};
			textBlock.PointerEnter += Polygon_PointerEnter;
			textBlock.PointerLeave += Polygon_PointerLeave;
			textBlock.Tapped += TextBox_Tapped;
			/*Border border = new Border()
			{
				BorderThickness = new Thickness(5),
				BorderBrush = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor),
				Child = textBlock,
			};
			this.Children.Add(border);*/
			this.Children.Add(textBlock);
		}

		private void TextBox_Tapped(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
		}

		public void UpdateSeries(OxyPlot.Series.LineSeries lineSeries)
		{
			this.series = lineSeries;
			if (IsChecked == true)
			{
				lineSeries.ItemsSource = lineSeries.ItemsSource ?? ItemsSource; // never gonna let you go...
				//ItemsSource = null;
				lineSeries.LineStyle = LineStyle.Solid;
				lineSeries.MarkerType = MarkerType.Circle;
				lineSeries.Selectable = true;
			}
			else
			{
				ItemsSource = lineSeries.ItemsSource ?? ItemsSource;
				lineSeries.ItemsSource = null;
				lineSeries.LineStyle = LineStyle.None;
				lineSeries.MarkerType = MarkerType.None;
				lineSeries.Selectable = false;
				//lineSeries.SelectionMode = OxyPlot.SelectionMode.
				lineSeries.Unselect();
			}
		}

		public void UpdateSeries(OxyPlot.Series.ScatterSeries scatterSeries)
		{
			this.series = scatterSeries;
			if (IsChecked == true)
			{
				scatterSeries.ItemsSource = scatterSeries.ItemsSource ?? ItemsSource; // never gonna let you go...
				//ItemsSource = null;
				scatterSeries.MarkerType = MarkerType.Circle;
				scatterSeries.Selectable = true;
			}
			else
			{
				ItemsSource = scatterSeries.ItemsSource ?? ItemsSource;
				scatterSeries.ItemsSource = null;
				scatterSeries.MarkerType = MarkerType.None;
				scatterSeries.Selectable = false;
				//lineSeries.SelectionMode = OxyPlot.SelectionMode.
				scatterSeries.Unselect();
			}
		}
	}
}
