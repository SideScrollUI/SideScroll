using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Avalonia;
using OxyPlot.Axes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using WeakEvent;

namespace Atlas.UI.Avalonia.Controls
{
	public class OxyListSeries
	{
		public ListSeries ListSeries { get; set; }
		public OxyPlot.Series.Series OxySeries { get; set; }
		public bool IsVisible { get; set; } = true;
		public bool IsSelected { get; set; }

		public override string ToString() => ListSeries.ToString();

		public OxyListSeries(ListSeries listSeries, OxyPlot.Series.Series oxySeries)
		{
			ListSeries = listSeries;
			OxySeries = oxySeries;
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

	public class MouseCursorMovedEventArgs : EventArgs
	{
		public double X { get; set; }

		public MouseCursorMovedEventArgs(double x)
		{
			X = x;
		}
	}

	public class TabControlChart : Grid, IDisposable
	{
		public int SeriesLimit { get; set; } = 25;
		private const double MarginPercent = 0.1; // This needs a min height so this can be lowered
		private const int MinSelectionWidth = 10;
		private static readonly OxyColor NowColor = OxyColors.Green;
		//private static OxyColor timeTrackerColor = Theme.TitleBackground;

		private readonly TabInstance TabInstance;
		//public ChartSettings ChartSettings { get; set; }
		public ListGroup ListGroup { get; set; }
		public bool FillHeight { get; set; }

		//private List<ListSeries> ListSeries { get; set; }
		public List<OxyListSeries> OxyListSeriesList = new List<OxyListSeries>();
		private Dictionary<string, OxyListSeries> IdxNameToSeries { get; set; } = new Dictionary<string, OxyListSeries>();
		private Dictionary<IList, ListSeries> ListToTabSeries { get; set; } = new Dictionary<IList, ListSeries>();
		private Dictionary<IList, int> ListToTabIndex { get; set; } = new Dictionary<IList, int>(); // not used
		public List<ListSeries> SelectedSeries
		{
			get
			{
				var selected = new List<ListSeries>();
				foreach (var oxyListSeries in OxyListSeriesList)
				{
					if (oxyListSeries.IsSelected)
						selected.Add(oxyListSeries.ListSeries);
				}
				if (selected.Count == OxyListSeriesList.Count && selected.Count > 1)
					selected.Clear(); // If all are selected, none are selected?
				return selected;
			}
		}
		public OxyPlot.Series.Series HoverSeries;

		//public SeriesCollection SeriesCollection { get; set; }

		public PlotModel PlotModel;
		public PlotView PlotView;
		private PropertyInfo xAxisPropertyInfo;
		public TabControlChartLegend Legend;
		public OxyPlot.Axes.Axis ValueAxis; // left/right?
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

		public static OxyColor GetColor(int index) => Colors[index % Colors.Length];

		public event EventHandler<SeriesSelectedEventArgs> OnSelectionChanged;

		private static readonly WeakEventSource<MouseCursorMovedEventArgs> _mouseCursorChangedEventSource = new WeakEventSource<MouseCursorMovedEventArgs>();
		public static event EventHandler<MouseCursorMovedEventArgs> OnMouseCursorChanged
		{
			add { _mouseCursorChangedEventSource.Subscribe(value); }
			remove { _mouseCursorChangedEventSource.Unsubscribe(value); }
		}

		public override string ToString() => ListGroup.ToString();

		public TabControlChart(TabInstance tabInstance, ListGroup listGroup, bool fillHeight = false)
		{
			TabInstance = tabInstance;
			ListGroup = listGroup;
			FillHeight = fillHeight;

			InitializeControls();
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
				MinWidth = 150,
				MinHeight = 80,
				[Grid.RowProperty] = 1,
			};

			// Show Hover text on mouse over instead of requiring holding the mouse down (why isn't this the default?)
			PlotView.ActualController.UnbindMouseDown(OxyMouseButton.Left); // remove default
			PlotView.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack); // show when hovering
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

			string title = ListGroup?.Name;
			if (title != null)
			{
				var titleTextBlock = new TextBlock()
				{
					Text = ListGroup?.Name,
					FontSize = 16,
					Foreground = Theme.BackgroundText,
					Margin = new Thickness(10, 5),
					//FontWeight = FontWeight.Medium,
					[Grid.ColumnSpanProperty] = 2,
				};
				if (!ListGroup.ShowOrder || ListGroup.Horizontal)
					titleTextBlock.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center;
				else
					titleTextBlock.Margin = new Thickness(40, 5, 5, 5);
				containerGrid.Children.Add(titleTextBlock);
			}

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

			OnMouseCursorChanged += TabControlChart_OnMouseCursorChanged;
			if (ListGroup.TimeWindow != null)
				ListGroup.TimeWindow.OnSelectionChanged += ListGroup_OnTimesChanged;

			Children.Add(containerGrid);

			Focusable = true;
		}

		private void UpdateVisible()
		{
			if (PlotView == null)
				return;

			bool visible = AvaloniaUtils.IsControlVisible(this);
			if (visible != PlotView.IsVisible)
			{
				PlotView.IsVisible = visible;
				Legend.IsVisible = visible;
				//PlotModel.InvalidatePlot(false);
				PlotView.InvalidateArrange();
				Legend.InvalidateArrange();
			}
		}

		// Anchor the chart to the top and stretch to max height, available size gets set to max :(
		protected override Size MeasureOverride(Size availableSize)
		{
			Size size = base.MeasureOverride(availableSize);
			if (FillHeight)
				size = new Size(size.Width, Math.Max(size.Height, Math.Min(MaxHeight, availableSize.Height)));
			return size;
		}

		public override void Render(DrawingContext context)
		{
			Dispatcher.UIThread.Post(UpdateVisible, DispatcherPriority.Background);
			base.Render(context);
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
				if (Chart.HoverSeries == series)
					return;

				if (series != null)
				{
					Chart.Legend.HighlightSeries(series);
				}
				else
				{
					Chart.Legend.UnhighlightAll(true);
				}
				Chart.HoverSeries = series;

				// todo: replace tracker here
			}
		}

		private void PlotView_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			if (HoverSeries != null)
			{
				HoverSeries = null;
				Legend.UnhighlightAll(true);
			}
		}

		private void Legend_OnSelectionChanged(object sender, EventArgs e)
		{
			StopSelecting();
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
			RecreatePlotModel();

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

			ListGroup.SortByTotal();
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
				(ListGroup.TimeWindow != null);

		private void AddAxis()
		{
			if (UseDateTimeAxis)
			{
				AddDateTimeAxis(ListGroup.TimeWindow);
				AddNowTime();
				if (ListGroup.ShowTimeTracker)
					AddTrackerLine();

				AddMouseListeners();
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

		public OxyPlot.Axes.DateTimeAxis AddDateTimeAxis(TimeWindow timeWindow = null)
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

			if (timeWindow != null)
			{
				UpdateDateTimeAxis(timeWindow);
			}

			PlotModel.Axes.Add(DateTimeAxis);
			return DateTimeAxis;
		}

		private void UpdateDateTimeAxis(TimeWindow timeWindow)
		{
			DateTimeAxis.Minimum = OxyPlot.Axes.DateTimeAxis.ToDouble(timeWindow.StartTime);
			DateTimeAxis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(timeWindow.EndTime);
			//DateTimeAxis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(endTime.AddSeconds(duration / 25.0)); // labels get clipped without this
			UpdateDateTimeInterval(timeWindow.Duration.TotalSeconds);
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

		public OxyPlot.Axes.Axis AddValueAxis(AxisPosition axisPosition = AxisPosition.Left, string key = null)
		{
			if (ListGroup.Logarithmic)
				ValueAxis = new OxyPlot.Axes.LogarithmicAxis();
			else
				ValueAxis = new OxyPlot.Axes.LinearAxis();

			ValueAxis.Position = axisPosition;
			ValueAxis.MajorGridlineStyle = LineStyle.Solid;
			ValueAxis.MajorGridlineColor = GridLineColor;
			ValueAxis.MinorGridlineStyle = LineStyle.None;
			ValueAxis.IntervalLength = 25;
			//ValueAxis.MinorStep = 20;
			//ValueAxis.MajorStep = 10;
			//ValueAxis.MinimumMinorStep = 20;
			ValueAxis.MinorTickSize = 0;
			ValueAxis.IsAxisVisible = true;
			ValueAxis.IsPanEnabled = false;
			ValueAxis.AxislineColor = GridLineColor;
			ValueAxis.AxislineStyle = LineStyle.Solid;
			ValueAxis.AxislineThickness = 2;
			ValueAxis.TickStyle = TickStyle.Outside;
			ValueAxis.TicklineColor = GridLineColor;
			//ValueAxis.MajorTickSize = 2;
			ValueAxis.MinorGridlineColor = OxyColors.Gray;
			ValueAxis.TitleColor = OxyColors.LightGray;
			ValueAxis.TextColor = OxyColors.LightGray;
			ValueAxis.LabelFormatter = ValueFormatter;

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

		public void UpdateValueAxis() // OxyPlot.Axes.LinearAxis valueAxis, string axisKey = null
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

						if (DateTimeAxis != null && (dataPoint.X < DateTimeAxis.Minimum || dataPoint.X > DateTimeAxis.Maximum))
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

			if (ListGroup.Logarithmic)
			{
				ValueAxis.Minimum = minimum * 0.85;
				ValueAxis.Maximum = maximum * 1.15;
			}
			else
			{
				var margin = (maximum - minimum) * MarginPercent;
				if (minimum == maximum)
					margin = Math.Abs(minimum);

				ValueAxis.Minimum = minimum - margin;
				ValueAxis.Maximum = maximum + margin;
			}
		}

		/*private void UpdateDateTimeAxis()
		{
			if (DateTimeAxis == null)
				return;

			//UpdateDateTimeInterval(double duration);
		}*/

		private static string ValueFormatter(double d)
		{
			double ad = Math.Abs(d);
			string prefix = "{0:#,0.###} ";
			if (ad >= 1E12)
			{
				return string.Format(prefix + "T", d / 1E12);
			}
			else if (ad >= 1E9)
			{
				return string.Format(prefix + "G", d / 1E9);
			}
			else if (ad >= 1E6)
			{
				return string.Format(prefix + "M", d / 1E6);
			}
			else if (ad >= 1E3)
			{
				return string.Format(prefix + "K", d / 1E3);
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

		private void ClearListeners()
		{
			if (Legend != null)
			{
				Legend.OnSelectionChanged -= Legend_OnSelectionChanged;
				Legend.OnVisibleChanged -= Legend_OnVisibleChanged;
			}

			OnMouseCursorChanged -= TabControlChart_OnMouseCursorChanged;
		}

		private void UnloadModel()
		{
			PlotView.Model = null;
			LinearAxis = null;
			DateTimeAxis = null;
			Legend?.Unload();
			ClearSeries();
			//if (plotModel != null)
			//	plotModel.Series.Clear();
			/*foreach (ListSeries listSeries in ChartSettings.ListSeries)
			{
				INotifyCollectionChanged iNotifyCollectionChanged = listSeries.iList as INotifyCollectionChanged;
				//if (iNotifyCollectionChanged != null)
				//	iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;
			}*/
		}

		private void ClearSeries()
		{
			PlotModel?.Series.Clear();

			OxyListSeriesList.Clear();
			ListToTabSeries.Clear();
			ListToTabIndex.Clear();
			IdxNameToSeries.Clear();
		}

		public void MergeGroup(ListGroup listGroup)
		{
			var prevListSeries = IdxNameToSeries;
			ClearSeries();
			ListGroup.Series = listGroup.Series;
			ListGroup.TimeWindow = listGroup.TimeWindow ?? ListGroup.TimeWindow;
			ListGroup.SortByTotal();
			foreach (var series in ListGroup.Series)
			{
				OxyColor? oxyColor = null;
				if (series.Name != null && prevListSeries.TryGetValue(series.Name, out OxyListSeries prevSeries))
					oxyColor = ((TabChartLineSeries)prevSeries.OxySeries).Color;

				AddSeries(series, oxyColor);
			}
		}

		public void AddSeries(ListSeries listSeries, OxyColor? oxyColor = null)
		{
			//if (listSeries.IsStacked)
			//	AddBarSeries(listSeries);
			//else
				AddListSeries(listSeries, oxyColor);
		}

		/*private void AddBarSeries(ListSeries listSeries)
		{
			var barSeries = new OxyPlot.Series.BarSeries
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

			plotModel.Series.Add(barSeries);

			//ListToTabSeries[listSeries.iList] = listSeries;
			//ListToTabIndex[listSeries.iList] = ListToTabIndex.Count;
		}*/

		public OxyPlot.Series.LineSeries AddListSeries(ListSeries listSeries, OxyColor? oxyColor = null)
		{
			if (OxyListSeriesList.Count >= SeriesLimit)
				return null;

			var lineSeries = new TabChartLineSeries(this, listSeries, UseDateTimeAxis)
			{
				Color = oxyColor ?? GetColor(PlotModel.Series.Count),
			};
			xAxisPropertyInfo = lineSeries.XAxisPropertyInfo;

			PlotModel.Series.Insert(0, lineSeries);

			var oxyListSeries = new OxyListSeries(listSeries, lineSeries);

			lineSeries.MouseDown += (s, e) =>
			{
				OnSelectionChanged?.Invoke(s, new SeriesSelectedEventArgs(new List<ListSeries>() { listSeries }));
				Legend.SelectSeries(lineSeries);
				e.Handled = true;
			};

			lineSeries.MouseUp += (s, e) =>
			{
				e.Handled = true; // Handle so zooming doesn't use?
			};

			OxyListSeriesList.Add(oxyListSeries);
			ListToTabSeries[listSeries.List] = listSeries;
			ListToTabIndex[listSeries.List] = ListToTabIndex.Count;
			if (listSeries.Name != null)
				IdxNameToSeries[listSeries.Name] = oxyListSeries;
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
			if (ListGroup.TimeWindow != null && ListGroup.TimeWindow.EndTime < now.AddMinutes(1))
				return;

			var annotation = new OxyPlot.Annotations.LineAnnotation
			{
				Type = LineAnnotationType.Vertical,
				X = OxyPlot.Axes.DateTimeAxis.ToDouble(now.ToUniversalTime()),
				Color = NowColor,
				// LineStyle = LineStyle.Dot, // doesn't work for vertical?
			};

			PlotModel.Annotations.Add(annotation);
		}

		private OxyPlot.Annotations.LineAnnotation _trackerAnnotation;
		private void AddTrackerLine()
		{
			_trackerAnnotation = new OxyPlot.Annotations.LineAnnotation
			{
				Type = LineAnnotationType.Vertical,
				//Color = Theme.TitleBackground.ToOxyColor(),
				//Color = Color.Parse("#21a094").ToOxyColor(),
				Color = Theme.GridBackgroundSelected.ToOxyColor(),
				//Color = timeTrackerColor,
				// LineStyle = LineStyle.Dot, // doesn't work for vertical?
			};

			PlotModel.Annotations.Add(_trackerAnnotation);
		}

		private void AddMouseListeners()
		{
			PlotModel.MouseDown += PlotModel_MouseDown;
			PlotModel.MouseMove += PlotModel_MouseMove;
			PlotModel.MouseUp += PlotModel_MouseUp;
			PlotModel.MouseLeave += PlotModel_MouseLeave;
		}

		private bool _selecting;
		private ScreenPoint _startScreenPoint;
		private DataPoint? _startDataPoint;
		private DataPoint? _endDataPoint;
		private OxyPlot.Annotations.RectangleAnnotation _rectangleAnnotation;

		private void UpdateMouseSelection(DataPoint endDataPoint)
		{
			if (_rectangleAnnotation == null)
			{
				_rectangleAnnotation = new OxyPlot.Annotations.RectangleAnnotation()
				{
					Fill = OxyColor.FromAColor((byte)Theme.ChartBackgroundSelectedAlpha, Theme.ChartBackgroundSelected.ToOxyColor()),
					Stroke = OxyColor.FromAColor((byte)180, Theme.ChartBackgroundSelected.ToOxyColor()),
					StrokeThickness = 1,
				};
			}
			if (!PlotModel.Annotations.Contains(_rectangleAnnotation))
				PlotModel.Annotations.Add(_rectangleAnnotation);

			_rectangleAnnotation.MinimumX = Math.Min(_startDataPoint.Value.X, endDataPoint.X);
			_rectangleAnnotation.MaximumX = Math.Max(_startDataPoint.Value.X, endDataPoint.X);
			_rectangleAnnotation.MinimumY = ValueAxis.Minimum;
			_rectangleAnnotation.MaximumY = ValueAxis.Maximum;

			Dispatcher.UIThread.Post(() => PlotModel.InvalidatePlot(false), DispatcherPriority.Background);
		}

		private void PlotModel_MouseDown(object sender, OxyMouseDownEventArgs e)
		{
			if (!_selecting || _startDataPoint == null)
			{
				_startDataPoint = OxyPlot.Axes.DateTimeAxis.InverseTransform(e.Position, DateTimeAxis, ValueAxis);
				_startScreenPoint = e.Position;
				_selecting = true;
				e.Handled = true;
			}
		}

		private void PlotModel_MouseMove(object sender, OxyMouseEventArgs e)
		{
			DataPoint dataPoint = OxyPlot.Axes.DateTimeAxis.InverseTransform(e.Position, DateTimeAxis, ValueAxis);
			var moveEvent = new MouseCursorMovedEventArgs(dataPoint.X);
			_mouseCursorChangedEventSource?.Raise(sender, moveEvent);

			if (_selecting && _startDataPoint != null)
			{
				_endDataPoint = OxyPlot.Axes.DateTimeAxis.InverseTransform(e.Position, DateTimeAxis, ValueAxis);
				UpdateMouseSelection(_endDataPoint.Value);
			}
		}

		private void PlotModel_MouseUp(object sender, OxyMouseEventArgs e)
		{
			if (_selecting && _startDataPoint != null)
			{
				_endDataPoint = OxyPlot.Axes.DateTimeAxis.InverseTransform(e.Position, DateTimeAxis, ValueAxis);
				double width = Math.Abs(e.Position.X - _startScreenPoint.X);
				if (width > MinSelectionWidth)
				{
					// Zoom In
					double left = Math.Min(_startDataPoint.Value.X, _endDataPoint.Value.X);
					double right = Math.Max(_startDataPoint.Value.X, _endDataPoint.Value.X);

					DateTimeAxis.Minimum = Math.Max(left, DateTimeAxis.Minimum);
					DateTimeAxis.Maximum = Math.Min(right, DateTimeAxis.Maximum);

					DateTime startTime = OxyPlot.Axes.DateTimeAxis.ToDateTime(DateTimeAxis.Minimum);
					DateTime endTime = OxyPlot.Axes.DateTimeAxis.ToDateTime(DateTimeAxis.Maximum);
					var timeWindow = new TimeWindow(startTime, endTime).Trim();

					UpdateDateTimeAxis(timeWindow);
					ListGroup.TimeWindow.Select(timeWindow);
				}
				else if (ListGroup.TimeWindow?.Selection != null)
				{
					// Zoom Out
					UpdateDateTimeAxis(ListGroup.TimeWindow);
					ListGroup.TimeWindow.Select(null);
				}
				else
				{
					// Deselect All
					Legend.SetAllVisible(true, true);
				}
				StopSelecting();
			}
		}

		private void ListGroup_OnTimesChanged(object sender, TimeWindowEventArgs e)
		{
			UpdateDateTimeAxis(e.TimeWindow);
			UpdateValueAxis();
			ListGroup.SortByTotal();
			Legend.RefreshModel();

			PlotView.InvalidatePlot(true);
			PlotView.Model.InvalidatePlot(true);
		}

		private void StopSelecting()
		{
			if (_rectangleAnnotation != null)
				PlotModel.Annotations.Remove(_rectangleAnnotation);
			_selecting = false;
		}

		// Hide cursor when out of scope
		private void PlotModel_MouseLeave(object sender, OxyMouseEventArgs e)
		{
			var moveEvent = new MouseCursorMovedEventArgs(0);
			_mouseCursorChangedEventSource?.Raise(sender, moveEvent);
		}

		// Update mouse tracker
		private void TabControlChart_OnMouseCursorChanged(object sender, MouseCursorMovedEventArgs e)
		{
			if (sender == PlotView?.Controller || _trackerAnnotation == null)
				return;

			_trackerAnnotation.X = e.X;
			Dispatcher.UIThread.Post(() => PlotModel.InvalidatePlot(false), DispatcherPriority.Background);
		}

		/*private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			lock (PlotModel.SyncRoot)
			{
				//Update();
				int index = ListToTabIndex[(IList)sender];
				ListSeries listSeries = ListToTabSeries[(IList)sender];
				AddPoints((OxyPlot.Series.LineSeries)plotModel.Series[index], listSeries, e.NewItems);
			}

			Dispatcher.UIThread.InvokeAsync(() => PlotModel.InvalidatePlot(true), DispatcherPriority.Background);
		}*/

		public void Dispose()
		{
			ClearListeners();
			UnloadModel();
		}
	}
}
