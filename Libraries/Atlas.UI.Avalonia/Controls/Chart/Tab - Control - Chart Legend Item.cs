using Atlas.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using OxyPlot;
using OxyPlot.Avalonia;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabChartLegendItem : Grid
	{
		public event EventHandler<EventArgs> OnSelectionChanged;
		public event EventHandler<EventArgs> OnVisibleChanged;

		public TabControlChartLegend legend;
		public OxyListSeries oxyListSeries;
		public OxyPlot.Series.Series series;
		public ListGroup listGroup;
		//public string Label { get; set; }
		public TextBlock textBlock;
		public TextBlock textBlockSum;
		private Polygon polygon;
		private Color color = Colors.Green;
		private OxyColor oxyColor;
		private MarkerType markerType;

		public int _Index;
		public int Index
		{
			get => _Index;
			set
			{
				_Index = value;
				textBlock.Text = value.ToString() + ". " + series.Title;
			}
		}
		public int Count { get; set; }
		public double Sum { get; set; }

		private bool _IsChecked = true;
		public bool IsChecked
		{
			get
			{
				return _IsChecked;
			}
			set
			{
				oxyListSeries.IsVisible = value;
				_IsChecked = value;
				SetFilled(value);
			}
		}

		public IEnumerable ItemsSource { get; internal set; }

		public List<DataPoint> Points { get; internal set; }

		public override string ToString() => series.Title;

		public TabChartLegendItem(TabControlChartLegend legend, OxyListSeries oxyListSeries)
		{
			this.legend = legend;
			this.oxyListSeries = oxyListSeries;
			series = oxyListSeries.OxySeries;
			listGroup = legend.listGroup;
			InitializeControls();
		}

		private void InitializeControls()
		{
			//HorizontalAlignment = HorizontalAlignment.Right;
			ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto");
			RowDefinitions = new RowDefinitions("Auto");
			//Margin = new Thickness(6);
			Background = Theme.TabBackground;

			UpdateSum();
			AddCheckBox();
			AddTextBlock();
			if (listGroup.ShowOrder && !listGroup.Horizontal)
				AddSumTextBlock();

			PointerEnter += TabChartLegendItem_PointerEnter;
			PointerLeave += TabChartLegendItem_PointerLeave;
		}

		private void SetFilled(bool filled)
		{
			polygon.Fill = new SolidColorBrush(filled && Count > 0 ? color : Colors.Transparent);
		}

		private void UpdateSum()
		{
			Sum = 0;
			Count = 0;
			if (series is OxyPlot.Series.LineSeries lineSeries)
			{
				if (lineSeries.Points.Count > 0)
				{
					Count = lineSeries.Points.Count;
					foreach (DataPoint dataPoint in lineSeries.Points)
					{
						if (!double.IsNaN(dataPoint.Y))
							Sum += dataPoint.Y;
					}
				}
				else if (lineSeries.ItemsSource != null)
				{
					// todo: finish
					Count = lineSeries.ItemsSource.GetEnumerator().MoveNext() ? 1 : 0;
					Sum = Count;
				}
			}
			if (Sum > 100)
				Sum = Math.Round(Sum);
			if (series is OxyPlot.Series.ScatterSeries scatterSeries)
			{
				// todo: finish
				Count = Math.Max(scatterSeries.Points.Count, scatterSeries.ItemsSource.GetEnumerator().MoveNext() ? 1 : 0);
				Sum = Count;
			}
		}

		private void AddCheckBox()
		{
			if (series is OxyPlot.Series.LineSeries lineSeries)
			{
				oxyColor = lineSeries.Color;
				markerType = lineSeries.MarkerType;
			}
			if (series is OxyPlot.Series.ScatterSeries scatterSeries)
			{
				oxyColor = scatterSeries.MarkerFill;
				markerType = scatterSeries.MarkerType;
			}
			color = oxyColor.ToColor();

			int width = 13;
			int height = 13;
			polygon = new Polygon()
			{
				Width = 16,
				Height = 16,
				Stroke = Brushes.Black,
				StrokeThickness = 1.5,
			};
			if (Count > 0)
				polygon.Fill = new SolidColorBrush(color);
			else
				IsChecked = false;
			UpdatePolygonPoints(width, height);
			polygon.PointerPressed += Polygon_PointerPressed;
			Children.Add(polygon);
		}

		private void UpdatePolygonPoints(int width, int height)
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

		private void AddTextBlock()
		{
			textBlock = new TextBlock()
			{
				Text = series.Title,
				Foreground = Brushes.LightGray,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
				Margin = new Thickness(2, 2, 6, 2),
				//VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				[Grid.ColumnProperty] = 1,
			};
			if (Index > 0)
				textBlock.Text = Index.ToString() + ": " + textBlock.Text;
			textBlock.Tapped += TextBox_Tapped;
			Children.Add(textBlock);
		}

		private void AddSumTextBlock()
		{
			textBlockSum = new TextBlock()
			{
				Text = Sum.Formatted(),
				Foreground = Brushes.LightGray,
				Margin = new Thickness(10, 2, 6, 2),
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Right,
				[Grid.ColumnProperty] = 2,
			};
			textBlockSum.Tapped += TextBox_Tapped;
			Children.Add(textBlockSum);
		}

		private void Polygon_PointerPressed(object sender, PointerPressedEventArgs e)
		{
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			{
				IsChecked = !IsChecked;
				OnSelectionChanged?.Invoke(this, null);
			}
		}

		private bool highlight;
		public bool Highlight
		{
			get => highlight;
			set
			{
				if (value == highlight)
					return;

				highlight = value;
				if (highlight)
				{
					UpdatePolygonPoints(15, 15);
					if (series is OxyPlot.Series.LineSeries lineSeries)
					{
						highlight = true;
						SetFilled(true);
						UpdateVisible(lineSeries);
						legend.UpdateHighlight(true);
						OnVisibleChanged?.Invoke(this, null);
					}
					textBlock.Foreground = Theme.GridBackgroundSelected;
					if (textBlockSum != null)
						textBlockSum.Foreground = Theme.GridBackgroundSelected;
				}
				else
				{
					UpdatePolygonPoints(13, 13);
					if (series is OxyPlot.Series.LineSeries lineSeries)
					{
						highlight = false;
						UpdateVisible(lineSeries);
						SetFilled(IsChecked);
						legend.UpdateHighlight(false);
						OnVisibleChanged?.Invoke(this, null);
					}
					textBlock.Foreground = Brushes.LightGray;
					if (textBlockSum != null)
						textBlockSum.Foreground = Brushes.LightGray;
				}
			}
		}

		private void TabChartLegendItem_PointerEnter(object sender, PointerEventArgs e)
		{
			Highlight = true;
		}

		private void TabChartLegendItem_PointerLeave(object sender, PointerEventArgs e)
		{
			Highlight = false;
		}

		private void TextBox_Tapped(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
		}

		public void UpdateVisible(OxyPlot.Series.LineSeries lineSeries)
		{
			this.series = lineSeries;
			if (IsChecked == true || highlight)
			{
				if (Points != null)
				{
					lineSeries.Points.Clear();
					lineSeries.Points.AddRange(Points);
				}
				//lineSeries.ItemsSource = lineSeries.ItemsSource ?? ItemsSource; // never gonna let you go...
				//ItemsSource = null;
				lineSeries.LineStyle = LineStyle.Solid;
				lineSeries.MarkerType = markerType;
				lineSeries.Selectable = true;
			}
			else
			{
				if (lineSeries.Points.Count > 0)
				{
					Points = new List<DataPoint>();
					Points.AddRange(lineSeries.Points);
				}
				lineSeries.Points.Clear();
				//lineSeries.Points = new List<DataPoint>();
				//ItemsSource = lineSeries.ItemsSource ?? ItemsSource;
				//lineSeries.ItemsSource = null;
				lineSeries.LineStyle = LineStyle.None;
				lineSeries.MarkerType = MarkerType.None;
				lineSeries.Selectable = false;
				//lineSeries.SelectionMode = OxyPlot.SelectionMode.
				lineSeries.Unselect();
			}
		}

		public void UpdateVisible(OxyPlot.Series.ScatterSeries scatterSeries)
		{
			this.series = scatterSeries;
			if (IsChecked == true || highlight)
			{
				scatterSeries.ItemsSource = scatterSeries.ItemsSource ?? ItemsSource; // never gonna let you go...
				//ItemsSource = null;
				scatterSeries.MarkerType = markerType;
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

		public void UpdateHighlight(bool showFaded)
		{
			OxyColor newColor;
			if (highlight || !showFaded)
				newColor = oxyColor;
			else
				newColor = OxyColor.FromAColor(32, oxyColor);
			
			if (series is OxyPlot.Series.LineSeries lineSeries)
			{
				lineSeries.MarkerFill = newColor;
				lineSeries.Color = newColor;
			}
		}
	}
}
