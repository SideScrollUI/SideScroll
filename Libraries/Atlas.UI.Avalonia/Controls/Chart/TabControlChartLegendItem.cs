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

		public TabControlChartLegend Legend;
		public OxyListSeries OxyListSeries;
		public OxyPlot.Series.Series Series;
		public ListGroup ListGroup;
		//public string Label { get; set; }
		public TextBlock TextBlock;
		public TextBlock textBlockSum;

		private Polygon polygon;
		private Color color = Colors.Green;
		private OxyColor oxyColor;
		private MarkerType markerType;

		private int _index;
		public int Index
		{
			get => _index;
			set
			{
				_index = value;
				TextBlock.Text = value.ToString() + ". " + Series.Title;
			}
		}
		public int Count { get; set; }
		public double Total { get; set; }

		private bool _isChecked = true;
		public bool IsChecked
		{
			get
			{
				return _isChecked;
			}
			set
			{
				OxyListSeries.IsVisible = value;
				_isChecked = value;
				SetFilled(value);
			}
		}

		public IEnumerable ItemsSource { get; internal set; }

		public List<DataPoint> Points { get; internal set; }

		public override string ToString() => Series.Title;

		public TabChartLegendItem(TabControlChartLegend legend, OxyListSeries oxyListSeries)
		{
			Legend = legend;
			OxyListSeries = oxyListSeries;
			Series = oxyListSeries.OxySeries;
			ListGroup = legend.ListGroup;

			InitializeControls();
		}

		private void InitializeControls()
		{
			//HorizontalAlignment = HorizontalAlignment.Right;
			ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto");
			RowDefinitions = new RowDefinitions("Auto");
			//Margin = new Thickness(6);
			Background = Theme.TabBackground;

			UpdateTotal();
			AddCheckBox();
			AddTextBlock();
			if (ListGroup.ShowOrder && !ListGroup.Horizontal)
				AddSumTextBlock();

			PointerEnter += TabChartLegendItem_PointerEnter;
			PointerLeave += TabChartLegendItem_PointerLeave;
		}

		private void SetFilled(bool filled)
		{
			polygon.Fill = new SolidColorBrush(filled && Count > 0 ? color : Colors.Transparent);
		}

		private void UpdateTotal()
		{
			if (OxyListSeries.ListSeries != null)
			{
				Total = OxyListSeries.ListSeries.Total;
				Count = OxyListSeries.ListSeries.List.Count;
				return;
			}
			Total = 0;
			Count = 0;
			if (Series is OxyPlot.Series.LineSeries lineSeries)
			{
				if (lineSeries.Points.Count > 0)
				{
					Count = lineSeries.Points.Count;
					foreach (DataPoint dataPoint in lineSeries.Points)
					{
						if (!double.IsNaN(dataPoint.Y))
							Total += dataPoint.Y;
					}
				}
				else if (lineSeries.ItemsSource != null)
				{
					// todo: finish
					Count = lineSeries.ItemsSource.GetEnumerator().MoveNext() ? 1 : 0;
					Total = Count;
				}
			}
			if (Total > 100)
				Total = Math.Round(Total);
			if (Series is OxyPlot.Series.ScatterSeries scatterSeries)
			{
				// todo: finish
				Count = Math.Max(scatterSeries.Points.Count, scatterSeries.ItemsSource.GetEnumerator().MoveNext() ? 1 : 0);
				Total = Count;
			}
		}

		private void AddCheckBox()
		{
			if (Series is OxyPlot.Series.LineSeries lineSeries)
			{
				oxyColor = lineSeries.Color;
				markerType = lineSeries.MarkerType;
			}
			if (Series is OxyPlot.Series.ScatterSeries scatterSeries)
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
			TextBlock = new TextBlock()
			{
				Text = Series.Title,
				Foreground = Brushes.LightGray,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
				Margin = new Thickness(2, 2, 6, 2),
				//VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				[Grid.ColumnProperty] = 1,
			};
			if (Index > 0)
				TextBlock.Text = Index.ToString() + ": " + TextBlock.Text;
			TextBlock.Tapped += TextBox_Tapped;
			Children.Add(TextBlock);
		}

		private void AddSumTextBlock()
		{
			textBlockSum = new TextBlock()
			{
				Text = Total.Formatted(),
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

		private bool _highlight;
		public bool Highlight
		{
			get => _highlight;
			set
			{
				if (value == _highlight)
					return;

				_highlight = value;
				if (_highlight)
				{
					UpdatePolygonPoints(15, 15);
					if (Series is OxyPlot.Series.LineSeries lineSeries)
					{
						_highlight = true;
						SetFilled(true);
						UpdateVisible(lineSeries);
						Legend.UpdateHighlight(true);
						OnVisibleChanged?.Invoke(this, null);
					}
					TextBlock.Foreground = Theme.GridBackgroundSelected;
					if (textBlockSum != null)
						textBlockSum.Foreground = Theme.GridBackgroundSelected;
				}
				else
				{
					UpdatePolygonPoints(13, 13);
					if (Series is OxyPlot.Series.LineSeries lineSeries)
					{
						_highlight = false;
						UpdateVisible(lineSeries);
						SetFilled(IsChecked);
						Legend.UpdateHighlight(false);
						OnVisibleChanged?.Invoke(this, null);
					}
					TextBlock.Foreground = Brushes.LightGray;
					if (textBlockSum != null)
						textBlockSum.Foreground = Brushes.LightGray;
				}
			}
		}

		private void TabChartLegendItem_PointerEnter(object sender, PointerEventArgs e)
		{
			Legend.UnhighlightAll(false);
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
			Series = lineSeries;
			if (IsChecked == true || _highlight)
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
			Series = scatterSeries;
			if (IsChecked == true || Highlight)
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
			if (Highlight || !showFaded)
				newColor = oxyColor;
			else
				newColor = OxyColor.FromAColor(32, oxyColor);
			
			if (Series is OxyPlot.Series.LineSeries lineSeries)
			{
				lineSeries.MarkerFill = newColor;
				lineSeries.Color = newColor;
			}
		}
	}
}
