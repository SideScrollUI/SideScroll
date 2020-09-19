using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Avalonia;
using OxyPlot.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Linq;
using Avalonia;

namespace Atlas.UI.Avalonia.Controls
{
	public class OxyListSeries
	{
		public ListSeries ListSeries { get; set; }
		public OxyPlot.Series.Series OxySeries { get; set; }
		public bool IsVisible { get; set; }

		public OxyListSeries(ListSeries listSeries, OxyPlot.Series.Series oxySeries)
		{
			ListSeries = listSeries;
			OxySeries = oxySeries;
			IsVisible = true;
		}
	}

	public class SeriesSelectedEventArgs : EventArgs
	{
		public List<ListSeries> Series { get; set; }

		public SeriesSelectedEventArgs(List<ListSeries> series)
		{
			Series = series;
		}
	}

	public class TabControlChart : Grid //, IDisposable
	{
		public static int SeriesLimit { get; set; } = 25;
		private const double MarginPercent = 0.1; // This needs a min height so this can be lowered
		private static OxyColor nowColor = OxyColors.Green;

		private TabInstance TabInstance;
		//public ChartSettings ChartSettings { get; set; }
		public ListGroup ListGroup { get; set; }
		public bool FillHeight { get; set; }

		//private List<ListSeries> ListSeries { get; set; }
		public List<OxyListSeries> OxyListSeriesList = new List<OxyListSeries>();
		private Dictionary<IList, ListSeries> ListToTabSeries { get; set; } = new Dictionary<IList, ListSeries>();
		private Dictionary<IList, int> ListToTabIndex { get; set; } = new Dictionary<IList, int>(); // not used
		public List<ListSeries> SelectedSeries
		{
			get
			{
				var selected = new List<ListSeries>();
				foreach (var oxyListSeries in OxyListSeriesList)
				{
					if (oxyListSeries.IsVisible)
						selected.Add(oxyListSeries.ListSeries);
				}
				if (selected.Count == OxyListSeriesList.Count)
					selected.Clear(); // If all are selected, none are selected?
				return selected;
			}
		}
		private OxyPlot.Series.Series hoverSeries;

		//public SeriesCollection SeriesCollection { get; set; }

		public PlotModel PlotModel;
		public PlotView PlotView;
		private PropertyInfo xAxisPropertyInfo;
		public TabControlChartLegend Legend;
		public OxyPlot.Axes.LinearAxis ValueAxis; // left/right?
		private OxyPlot.Axes.CategoryAxis categoryAxis;

		public OxyPlot.Axes.LinearAxis LinearAxis;
		public OxyPlot.Axes.DateTimeAxis DateTimeAxis;

		private static OxyColor GridLineColor = OxyColor.Parse("#333333");
		public static OxyColor[] Colors { get; set; } = new OxyColor[]
		{
			OxyColors.LawnGreen,
			OxyColors.Fuchsia,
			OxyColors.Cyan,
			//OxyColors.Aquamarine, // too close to Cyan (but more matte)
			OxyColors.Gold,
			OxyColors.DodgerBlue,
			OxyColors.Red,
			OxyColors.BlueViolet,
			//OxyColors.SlateBlue,
			OxyColors.Orange,
			//OxyColors.Pink,
			//OxyColors.Coral,
			//OxyColors.YellowGreen,
			OxyColors.Salmon,
			OxyColors.MediumSpringGreen,
		};

		public static OxyColor GetColor(int index)
		{
			return Colors[index % Colors.Length];
		}

		public event EventHandler<SeriesSelectedEventArgs> OnSelectionChanged;

		public TabControlChart(TabInstance tabInstance, ListGroup listGroup, bool fillHeight = false)
		{
			TabInstance = tabInstance;
			ListGroup = listGroup;
			FillHeight = fillHeight;

			InitializeControls();
		}

		public override string ToString()
		{
			return ListGroup.ToString(); // todo: fix for multiple
		}

		private void InitializeControls()
		{
			HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch; // OxyPlot import collision
			if (FillHeight)
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
			else
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
			ColumnDefinitions = new ColumnDefinitions("*");
			RowDefinitions = new RowDefinitions("*");
			MaxWidth = 1500;
			MaxHeight = 645; // 25 Items

			if (TabInstance.TabViewSettings.ChartDataSettings.Count == 0)
				TabInstance.TabViewSettings.ChartDataSettings.Add(new TabDataSettings());

			PlotView = new PlotView()
			{
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,

				Background = Brushes.Transparent,
				BorderBrush = Brushes.LightGray,
				IsMouseWheelEnabled = false,
				//DisconnectCanvasWhileUpdating = false, // Tracker will show behind grid lines if the PlotView is resized and this is set
				MinHeight = 100,
				MinWidth = 150,
				[Grid.RowProperty] = 1,
			};

			// Show Hover text on mouse over instead of requiring holding the mouse down (why isn't this the default?)
			PlotView.ActualController.UnbindMouseDown(OxyMouseButton.Left); // remove default
			PlotView.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack); // show when hovering
			PlotView.PointerPressed += PlotView_PointerPressed;
			PointerLeave += PlotView_PointerLeave; // doesn't work on PlotView

			PlotView.ActualController.BindMouseEnter(new DelegatePlotCommand<OxyMouseEventArgs>(
				(view, controller, args) =>
				controller.AddHoverManipulator(view, new MouseHoverManipulator(this), args)));

			LoadPlotModel();
			/*plotView.Template = new ControlTemplate() // todo: fix
			{
				Content = new object(),
				TargetType = typeof(object),
			};*/

			var containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,Auto"),
				RowDefinitions = new RowDefinitions("Auto,*,Auto"),
				HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,
				Background = Theme.TabBackground, // grid lines look bad when hovering without this
			};

			var title = new TextBlock()
			{
				Text = ListGroup?.Name,
				FontSize = 16,
				//Foreground = Brushes.LightGray,
				//Foreground = Theme.ToolbarTextForeground,
				Foreground = Theme.BackgroundText,
				Margin = new Thickness(10, 5),
				//FontWeight = FontWeight.Medium,
				[Grid.ColumnSpanProperty] = 2,
			};
			if (!ListGroup.ShowOrder || ListGroup.Horizontal)
				title.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center;
			else
				title.Margin = new Thickness(40, 5, 5, 5);
			containerGrid.Children.Add(title);

			containerGrid.Children.Add(PlotView);

			Legend = new TabControlChartLegend(this);
			if (ListGroup.Horizontal)
			{
				// Bottom
				SetRow(Legend, 2);
				Legend.MaxHeight = 100;
			}
			else
			{
				// Right Side
				SetRow(Legend, 1);
				SetColumn(Legend, 1);
				Legend.MaxWidth = 300;
			}
			containerGrid.Children.Add(Legend);
			Legend.OnSelectionChanged += Legend_OnSelectionChanged;
			Legend.OnVisibleChanged += Legend_OnVisibleChanged;

			Children.Add(containerGrid);

			Focusable = true;
		}

		// Anchor the chart to the top and stretch to max height, available size gets set to max :(
		protected override Size MeasureOverride(Size availableSize)
		{
			Size size = base.MeasureOverride(availableSize);
			if (FillHeight)
				size = new Size(size.Width, Math.Max(size.Height, Math.Min(MaxHeight, availableSize.Height)));
			return size;
		}

		public class MouseHoverManipulator : TrackerManipulator
		{
			public TabControlChart Chart;

			public MouseHoverManipulator(TabControlChart chart)
				: base(chart.PlotView)
			{
				Chart = chart;
				LockToInitialSeries = false;
				Snap = true;
				PointsOnly = false;
			}

			public override void Delta(OxyMouseEventArgs e)
			{
				base.Delta(e);

				var series = PlotView.ActualModel.GetSeriesFromPoint(e.Position, 20);
				if (Chart.hoverSeries == series)
					return;

				if (series != null)
				{
					Chart.Legend.HighlightSeries(series);
				}
				else
				{
					Chart.Legend.UnhighlightAll();
				}
				Chart.hoverSeries = series;

				// todo: replace tracker here
			}
		}

		private void PlotView_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			if (hoverSeries != null)
			{
				hoverSeries = null;
				Legend.UnhighlightAll();
			}
		}

		private void PlotView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e)
		{
			Legend.SetAllVisible(true, true);
		}

		private void Legend_OnSelectionChanged(object sender, EventArgs e)
		{
			UpdateValueAxis();
			OnSelectionChanged?.Invoke(sender, new SeriesSelectedEventArgs(SelectedSeries));
		}

		private void Legend_OnVisibleChanged(object sender, EventArgs e)
		{
			UpdateValueAxis();
		}

		public void LoadListGroup(ListGroup listGroup)
		{
			ListGroup = listGroup;
			LoadPlotModel();
			Refresh();
		}

		public void LoadPlotModel()
		{
			UnloadModel();
			RecreatePlotModel();

			foreach (ListSeries listSeries in ListGroup.Series)
			{
				AddSeries(listSeries);
			}

			// would need to be able to disable to use
			//foreach (ListSeries listSeries in ChartSettings.ListSeries)
			//	AddSeries(listSeries);

			AddAxis();
			UpdateValueAxis();
			UpdateLinearAxis();

			PlotView.Model = PlotModel;
			IsVisible = true;
		}

		public void RecreatePlotModel()
		{
			UnloadModel();
			PlotModel = new PlotModel()
			{
				//Title = ListGroup?.Name,
				//TitleFontWeight = 400,
				//TitleFontSize = 16,
				//TitleFont = "Arial",
				IsLegendVisible = false,
				LegendPlacement = LegendPlacement.Outside,
				//LegendTitleColor = OxyColors.Yellow, // doesn't work

				TitleColor = OxyColors.LightGray,
				//PlotAreaBorderColor = OxyColors.LightGray,
				PlotAreaBorderColor = OxyColor.Parse("#888888"),
				TextColor = OxyColors.Black,
				LegendTextColor = OxyColors.LightGray,
				SelectionColor = OxyColors.Blue,
			};
			PlotView.Model = PlotModel;
		}

		public void Refresh()
		{
			UpdateValueAxis();
			UpdateLinearAxis();
			Legend.RefreshModel();
			PlotView.InvalidatePlot(true);
			PlotView.Model.InvalidatePlot(true);
		}

		public void Unload()
		{
			IsVisible = false;
			UnloadModel();
		}

		private bool UseDateTimeAxis => (xAxisPropertyInfo?.PropertyType == typeof(DateTime)) ||
				(ListGroup.StartTime != null && ListGroup.EndTime != null);

		private void AddAxis()
		{
			if (UseDateTimeAxis)
			{
				AddDateTimeAxis(ListGroup.StartTime, ListGroup.EndTime);
				AddNowTime();
				//AddTrackerLine();
			}
			else
			{
				AddLinearAxis();
			}
			if (ListGroup.Series.Count > 0 && ListGroup.Series[0].IsStacked)
				AddCategoryAxis();
			else
				AddValueAxis();
		}

		public OxyPlot.Axes.DateTimeAxis AddDateTimeAxis(DateTime? startTime = null, DateTime? endTime = null)
		{
			DateTimeAxis = new OxyPlot.Axes.DateTimeAxis
			{
				Position = AxisPosition.Bottom,
				//MinorIntervalType = DateTimeIntervalType.Days,
				//IntervalType = DateTimeIntervalType.Days,
				IntervalType = DateTimeIntervalType.Hours,
				MajorGridlineStyle = LineStyle.Solid,
				MajorGridlineColor = GridLineColor,
				//MinorGridlineStyle = LineStyle.None,
				IntervalLength = 75,
				IsAxisVisible = true,
				IsPanEnabled = false,
				AxislineColor = OxyColors.Black,
				//AxislineColor = GridLineColor,
				AxislineStyle = LineStyle.Solid,
				AxislineThickness = 2,
				TickStyle = TickStyle.Outside,
				TicklineColor = GridLineColor,
				//MajorTickSize = 5,
				MinorGridlineColor = OxyColors.Gray,
				//MinorTicklineColor = GridLineColor,
				//MinorTickSize = 5,
				AxisTickToLabelDistance = 2,
				//MinimumMajorStep = TimeSpan.FromSeconds(1).TotalDays,
				TitleColor = OxyColors.LightGray,
				TextColor = OxyColors.LightGray,
			};
			if (startTime != null && endTime != null)
			{
				double duration = endTime.Value.Subtract(startTime.Value).TotalSeconds;
				DateTimeAxis.Minimum = OxyPlot.Axes.DateTimeAxis.ToDouble(startTime.Value);
				DateTimeAxis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(endTime.Value);
				//DateTimeAxis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(endTime.Value.AddSeconds(duration / 25.0)); // labels get clipped without this
				UpdateDateTimeInterval(duration);
			}
			PlotModel.Axes.Add(DateTimeAxis);
			return DateTimeAxis;
		}

		private void UpdateDateTimeInterval(double duration)
		{
			var dateFormat = GetDateTimeFormat(duration);
			DateTimeAxis.StringFormat = dateFormat.TextFormat;
			DateTimeAxis.MinimumMajorStep = dateFormat.StepSize.TotalDays;
			double widthPerLabel = 6 * DateTimeAxis.StringFormat.Length + 25;
			DateTimeAxis.IntervalLength = Math.Max(50, widthPerLabel);
		}

		private void AddLinearAxis()
		{
			LinearAxis = new OxyPlot.Axes.LinearAxis
			{
				Position = AxisPosition.Bottom,
				MajorGridlineStyle = LineStyle.Solid,
				MajorGridlineColor = GridLineColor,
				TitleColor = OxyColors.LightGray,
				TextColor = OxyColors.LightGray,
				TicklineColor = GridLineColor,
				MinorGridlineColor = OxyColors.Gray,
			};
			PlotModel.Axes.Add(LinearAxis);
		}

		public OxyPlot.Axes.LinearAxis AddValueAxis(AxisPosition axisPosition = AxisPosition.Left, string key = null)
		{
			ValueAxis = new OxyPlot.Axes.LinearAxis
			{
				Position = axisPosition,
				MajorGridlineStyle = LineStyle.Solid,
				MajorGridlineColor = GridLineColor,
				MinorGridlineStyle = LineStyle.None,
				IntervalLength = 25,
				//MinorStep = 20,
				//MajorStep = 10,
				//MinimumMinorStep = 20,
				MinorTickSize = 0,
				IsAxisVisible = true,
				IsPanEnabled = false,
				AxislineColor = GridLineColor,
				AxislineStyle = LineStyle.Solid,
				AxislineThickness = 2,
				TickStyle = TickStyle.Outside,
				TicklineColor = GridLineColor,
				//MajorTickSize = 2,
				MinorGridlineColor = OxyColors.Gray,
				TitleColor = OxyColors.LightGray,
				TextColor = OxyColors.LightGray,
				LabelFormatter = ValueFormatter,
			};
			if (key != null)
				ValueAxis.Key = key;
			PlotModel.Axes.Add(ValueAxis);
			return ValueAxis;
		}

		public OxyPlot.Axes.CategoryAxis AddCategoryAxis(AxisPosition axisPosition = AxisPosition.Left, string key = null)
		{
			categoryAxis = new OxyPlot.Axes.CategoryAxis
			{
				Position = axisPosition,
				IntervalLength = 20,
				MajorGridlineStyle = LineStyle.Solid,
				MajorGridlineColor = GridLineColor,
				MinorGridlineStyle = LineStyle.None,
				MinorTickSize = 0,
				MinorStep = 20,
				MinimumMinorStep = 10,
				IsAxisVisible = true,
				IsPanEnabled = false,
				AxislineColor = GridLineColor,
				AxislineStyle = LineStyle.Solid,
				AxislineThickness = 2,
				TickStyle = TickStyle.Outside,
				TicklineColor = GridLineColor,
				//MajorTickSize = 2,
				MinorGridlineColor = OxyColors.Gray,
				TitleColor = OxyColors.LightGray,
				TextColor = OxyColors.LightGray,
				LabelFormatter = ValueFormatter,
			};
			if (key != null)
				categoryAxis.Key = key;

			foreach (ListSeries listSeries in ListGroup.Series)
			{
				categoryAxis.Labels.Add(listSeries.Name);
			}

			PlotModel.Axes.Add(categoryAxis);
			return categoryAxis;
		}

		private void UpdateLinearAxis()
		{
			if (LinearAxis == null)
				return;

			double minimum = double.MaxValue;
			double maximum = double.MinValue;

			foreach (OxyPlot.Series.Series series in PlotModel.Series)
			{
				if (series is OxyPlot.Series.LineSeries lineSeries)
				{
					if (lineSeries.LineStyle == LineStyle.None)
						continue;

					foreach (var dataPoint in lineSeries.Points)
					{
						double x = dataPoint.X;
						if (double.IsNaN(x))
							continue;

						minimum = Math.Min(minimum, x);
						maximum = Math.Max(maximum, x);
					}
				}
			}

			if (minimum == double.MaxValue)
			{
				// didn't find any values
				minimum = 0;
				maximum = 1;
			}

			LinearAxis.Minimum = minimum;
			LinearAxis.Maximum = maximum;
		}

		private void UpdateValueAxis() // OxyPlot.Axes.LinearAxis valueAxis, string axisKey = null
		{
			if (ValueAxis == null)
				return;

			double minimum = double.MaxValue;
			double maximum = double.MinValue;
			bool hasFraction = false;

			foreach (OxyPlot.Series.Series series in PlotModel.Series)
			{
				if (series is OxyPlot.Series.LineSeries lineSeries)
				{
					if (lineSeries.LineStyle == LineStyle.None)
						continue;

					foreach (var dataPoint in lineSeries.Points)
					{
						double y = dataPoint.Y;
						if (double.IsNaN(y))
							continue;

						hasFraction |= (y % 1 != 0.0);

						minimum = Math.Min(minimum, y);
						maximum = Math.Max(maximum, y);
					}
				}
				if (series is OxyPlot.Series.ScatterSeries scatterSeries)
				{
					if (scatterSeries.ItemsSource == null)
						continue;

					//if (axisKey == "right" && lineSeries.YAxisKey != "right")
					//	continue;

					PropertyInfo propertyInfo = null;
					foreach (var item in scatterSeries.ItemsSource)
					{
						if (propertyInfo == null)
							propertyInfo = item.GetType().GetProperty(scatterSeries.DataFieldY);

						var value = propertyInfo.GetValue(item);
						double d = Convert.ToDouble(value);
						if (double.IsNaN(d))
							continue;

						minimum = Math.Min(minimum, d);
						maximum = Math.Max(maximum, d);
					}
				}
			}

			if (minimum == double.MaxValue)
			{
				// didn't find any values
				minimum = 0;
				maximum = 1;
			}

			foreach (OxyPlot.Annotations.Annotation annotation in PlotModel.Annotations)
			{
				if (annotation is OxyPlot.Annotations.LineAnnotation lineAnnotation)
					maximum = Math.Max(lineAnnotation.Y * 1.1, maximum);
			}

			ValueAxis.MinimumMajorStep = hasFraction ? 0 : 1;

			if (ListGroup.MinValue is double minValue)
				minimum = minValue;

			var margin = (maximum - minimum) * MarginPercent;
			if (minimum == maximum)
				margin = Math.Abs(minimum);

			ValueAxis.Minimum = minimum - margin;
			ValueAxis.Maximum = maximum + margin;
		}

		private void UpdateDateTimeAxis()
		{
			if (DateTimeAxis == null)
				return;

			//UpdateDateTimeInterval(double duration);
		}

		private static string ValueFormatter(double d)
		{
			double ad = Math.Abs(d);
			if (ad >= 1E12)
			{
				return string.Format("{0} T", d / 1E12);
			}
			else if (ad >= 1E9)
			{
				return string.Format("{0} G", d / 1E9);
			}
			else if (ad >= 1E6)
			{
				return string.Format("{0} M", d / 1E6);
			}
			else if (ad >= 1E3)
			{
				return string.Format("{0} K", d / 1E3);
			}
			else
			{
				return d.Formatted();
			}
		}

		public class DateTimeFormat
		{
			public double Maximum { get; set; }
			public TimeSpan StepSize { get; set; }
			public string TextFormat { get; set; }

			public DateTimeFormat(double maximum, TimeSpan stepSize, string textFormat)
			{
				Maximum = maximum;
				StepSize = stepSize;
				TextFormat = textFormat;
			}
		}

		public List<DateTimeFormat> DateFormats = new List<DateTimeFormat>
		{
			new DateTimeFormat(2 * 60, TimeSpan.FromSeconds(1), "H:mm:ss"),
			new DateTimeFormat(24 * 60 * 60, TimeSpan.FromMinutes(1), "H:mm"),
			new DateTimeFormat(3 * 24 * 60 * 60, TimeSpan.FromMinutes(1), "M/d H:mm"),
			new DateTimeFormat(6 * 30 * 24 * 60 * 60, TimeSpan.FromDays(1), "M/d"),
			new DateTimeFormat(1000.0 * 12 * 30 * 24 * 60 * 60, TimeSpan.FromDays(1), "yyyy-M-d"),
		};

		public DateTimeFormat GetDateTimeFormat(double duration)
		{
			foreach (var format in DateFormats)
			{
				if (duration < format.Maximum)
					return format;
			}

			return null;
		}

		private void UnloadModel()
		{
			PlotView.Model = null;
			LinearAxis = null;
			DateTimeAxis = null;
			Legend?.Unload();
			OxyListSeriesList.Clear();
			ListToTabSeries.Clear();
			ListToTabIndex.Clear();
			//if (plotModel != null)
			//	plotModel.Series.Clear();
			/*foreach (ListSeries listSeries in ChartSettings.ListSeries)
			{
				INotifyCollectionChanged iNotifyCollectionChanged = listSeries.iList as INotifyCollectionChanged;
				//if (iNotifyCollectionChanged != null)
				//	iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;
			}*/
		}

		public void MergeGroup(ListGroup listGroup)
		{
			listGroup.SortBySum();
			foreach (var series in listGroup.Series)
				AddSeries(series);
		}

		public void AddSeries(ListSeries listSeries)
		{
			//if (listSeries.IsStacked)
			//	AddBarSeries(listSeries);
			//else
				AddListSeries(listSeries);
		}

		private void AddBarSeries(ListSeries listSeries)
		{
			/*var barSeries = new OxyPlot.Series.BarSeries
			{
				Title = listSeries.Name,
				StrokeThickness = 2,
				FillColor = GetColor(plotModel.Series.Count),
				TextColor = OxyColors.Black,
				IsStacked = listSeries.IsStacked,
				TrackerFormatString = "{0}\nTime: {2:yyyy-M-d H:mm:ss.FFF}\nValue: {4:#,0.###}",
			};
			var dataPoints = GetDataPoints(listSeries, listSeries.iList);
			foreach (DataPoint dataPoint in dataPoints)
			{
				barSeries.Items.Add(new BarItem(dataPoint.X, (int)dataPoint.Y));
			}

			plotModel.Series.Add(barSeries);*/

			/*ListToTabSeries[listSeries.iList] = listSeries;
			ListToTabIndex[listSeries.iList] = ListToTabIndex.Count;*/
		}

		public OxyPlot.Series.LineSeries AddListSeries(ListSeries listSeries)
		{
			if (OxyListSeriesList.Count >= SeriesLimit)
				return null;

			var lineSeries = new TabChartLineSeries(this, listSeries, UseDateTimeAxis)
			{
				Color = GetColor(PlotModel.Series.Count),
			};
			xAxisPropertyInfo = lineSeries.xAxisPropertyInfo;

			PlotModel.Series.Add(lineSeries);

			var oxyListSeries = new OxyListSeries(listSeries, lineSeries);

			lineSeries.MouseDown += (s, e) =>
			{
				OnSelectionChanged?.Invoke(s, new SeriesSelectedEventArgs(new List<ListSeries>() { listSeries }));
				Legend.SelectSeries(lineSeries);
				e.Handled = true;
			};

			OxyListSeriesList.Add(oxyListSeries);
			ListToTabSeries[listSeries.iList] = listSeries;
			ListToTabIndex[listSeries.iList] = ListToTabIndex.Count;
			return lineSeries;
		}

		private void TabData_OnSelectionChanged(object sender, EventArgs e)
		{
			UnloadModel();
			LoadPlotModel();
		}

		private void AddNowTime()
		{
			var now = DateTime.UtcNow;
			if (ListGroup.EndTime < now.AddMinutes(1))
				return;
			var annotation = new OxyPlot.Annotations.LineAnnotation
			{
				Type = LineAnnotationType.Vertical,
				X = OxyPlot.Axes.DateTimeAxis.ToDouble(now.ToUniversalTime()),
				Color = nowColor,
				// LineStyle = LineStyle.Dot, // doesn't work for vertical?
			};

			PlotModel.Annotations.Add(annotation);
		}

		private OxyPlot.Annotations.LineAnnotation trackerAnnotation;
		private void AddTrackerLine()
		{
			var now = DateTime.UtcNow;
			if (ListGroup.EndTime < now.AddMinutes(1))
				return;
			trackerAnnotation = new OxyPlot.Annotations.LineAnnotation
			{
				Type = LineAnnotationType.Vertical,
				//X = OxyPlot.Axes.DateTimeAxis.ToDouble(now.ToUniversalTime()),
				Color = nowColor,
				// LineStyle = LineStyle.Dot, // doesn't work for vertical?
			};

			PlotModel.Annotations.Add(trackerAnnotation);
		}

		/*private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			lock (this.plotModel.SyncRoot)
			{
				//this.Update();
				int index = ListToTabIndex[(IList)sender];
				ListSeries listSeries = ListToTabSeries[(IList)sender];
				AddPoints((OxyPlot.Series.LineSeries)plotModel.Series[index], listSeries, e.NewItems);
			}

			Dispatcher.UIThread.InvokeAsync(() => this.plotModel.InvalidatePlot(true), DispatcherPriority.Background);
		}*/

		public void Dispose()
		{
			UnloadModel();
		}
	}
}
