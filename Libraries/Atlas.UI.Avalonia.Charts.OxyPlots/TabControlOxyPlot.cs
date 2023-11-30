using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Themes;
using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Avalonia;
using OxyPlot.Axes;
using System.Reflection;

namespace Atlas.UI.Avalonia.Charts.OxyPlots;

public class OxyPlotCreator : IControlCreator
{
	public static void Register()
	{
		TabView.ControlCreators[typeof(ChartView)] = new OxyPlotCreator();
	}

	public void AddControl(TabInstance tabInstance, TabControlSplitContainer container, object obj)
	{
		var chartView = (ChartView)obj;

		var tabChart = new TabControlOxyPlot(tabInstance, chartView, true);

		container.AddControl(tabChart, true, SeparatorType.Spacer);
	}
}

public class MouseHoverManipulator : TrackerManipulator
{
	public TabControlOxyPlot Chart;

	public MouseHoverManipulator(TabControlOxyPlot chart)
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
			Chart.Legend.HighlightSeries(series.Title);
		}
		else
		{
			Chart.Legend.UnhighlightAll(true);
		}
		Chart.HoverSeries = series;

		// todo: replace tracker here
	}
}

public class TabControlOxyPlot : TabControlChart<OxyPlotLineSeries>, IDisposable
{
	public static OxyColor TimeTrackerOxyColor { get; set; } = TimeTrackerColor.ToOxyColor();
	public static OxyColor GridLineOxyColor { get; set; } = GridLineColor.ToOxyColor();
	public static OxyColor TextOxyColor { get; set; } = TextColor.ToOxyColor();

	public PlotView PlotView;
	public PlotModel? PlotModel;
	public TabControlOxyPlotLegend Legend;
	public OxyPlot.Axes.Axis? ValueAxis; // left/right?
	public OxyPlot.Axes.CategoryAxis? CategoryAxis;

	public OxyPlot.Axes.LinearAxis? LinearAxis;
	public OxyPlot.Axes.DateTimeAxis? DateTimeAxis;
	public OxyPlot.Axes.Axis? XAxis => DateTimeAxis ?? LinearAxis;

	private OxyPlot.Annotations.LineAnnotation? _trackerAnnotation;

	private bool _selecting;
	private ScreenPoint _startScreenPoint;
	private DataPoint? _startDataPoint;
	private DataPoint? _endDataPoint;
	private OxyPlot.Annotations.RectangleAnnotation? _zoomAnnotation;

	public OxyPlot.Series.Series? HoverSeries;

	public TabControlOxyPlot(TabInstance tabInstance, ChartView chartView, bool fillHeight = false) :
		base(tabInstance, chartView, fillHeight)
	{
		PlotView = new PlotView()
		{
			HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
			VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,

			Background = Brushes.Transparent,
			IsMouseWheelEnabled = false,
			ClipToBounds = false,
			//DisconnectCanvasWhileUpdating = false, // Tracker will show behind grid lines if the PlotView is resized and this is set
			MinWidth = 150,
			MinHeight = 80,
			[Grid.RowProperty] = 1,
		};
		ClipToBounds = true; // Slows things down too much without this, could possible change while tracker visible?

		// Show Hover text on mouse over instead of requiring holding the mouse down (why isn't this the default?)
		PlotView.ActualController.UnbindMouseDown(OxyMouseButton.Left); // remove default
		PlotView.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack); // show when hovering
		PlotView.EffectiveViewportChanged += Chart_EffectiveViewPortChanged;
		PointerExited += PlotView_PointerExited; // doesn't work on PlotView

		PlotView.AddHandler(KeyDownEvent, PlotView_KeyDown, RoutingStrategies.Tunnel);

		PlotView.ActualController.BindMouseEnter(new DelegatePlotCommand<OxyMouseEventArgs>(
			(view, controller, args) =>
				controller.AddHoverManipulator(view, new MouseHoverManipulator(this), args)));

		ReloadView();

		var containerGrid = new Grid()
		{
			ColumnDefinitions = new ColumnDefinitions("*,Auto"),
			RowDefinitions = new RowDefinitions("Auto,*,Auto"),
			HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
			VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,
			Background = AtlasTheme.TabBackground, // grid lines look bad when hovering without this
		};

		if (TitleTextBlock != null)
		{
			containerGrid.Children.Add(TitleTextBlock);
		}
		else
		{
			containerGrid.RowDefinitions[0].Height = new GridLength(0);
		}

		containerGrid.Children.Add(PlotView);

		Legend = new TabControlOxyPlotLegend(this);
		if (ChartView!.LegendPosition == ChartLegendPosition.Bottom)
		{
			SetRow(Legend, 2);
			Legend.MaxHeight = 100;
		}
		else if (ChartView!.LegendPosition == ChartLegendPosition.Right)
		{
			SetRow(Legend, 1);
			SetColumn(Legend, 1);
			Legend.MaxWidth = 300;
		}
		containerGrid.Children.Add(Legend);
		Legend.OnSelectionChanged += Legend_OnSelectionChanged;
		Legend.OnVisibleChanged += Legend_OnVisibleChanged;

		OnMouseCursorChanged += TabControlOxyPlot_OnMouseCursorChanged;
		if (ChartView.TimeWindow != null)
		{
			ChartView.TimeWindow.OnSelectionChanged += ListGroup_OnTimesChanged;
		}

		Children.Add(containerGrid);
	}

	public void LoadView(ChartView chartView)
	{
		ChartView = chartView;
		ReloadView();
		Refresh();
	}

	public override void UpdateView(ChartView chartView)
	{
		ClearSeries();

		ChartView.Series = chartView.Series;
		ChartView.TimeWindow = chartView.TimeWindow ?? ChartView.TimeWindow;
		ChartView.SortByTotal();

		foreach (var series in ChartView.Series)
		{
			AddSeries(series);
		}
	}

	public override void ReloadView()
	{
		RecreatePlotModel();

		IsVisible = true;
	}

	public void RecreatePlotModel()
	{
		UnloadModel();
		PlotModel = new PlotModel()
		{
			IsLegendVisible = false,

			TextColor = OxyColors.Black,
			SelectionColor = OxyColors.Blue,
			PlotAreaBorderThickness = new OxyThickness(0),
		};

		ChartView.SortByTotal();
		foreach (ListSeries listSeries in ChartView.Series)
		{
			AddSeries(listSeries);
		}
		 
		AddAxis();

		foreach (ChartAnnotation chartAnnotation in ChartView.Annotations)
		{
			AddAnnotation(chartAnnotation);
		}

		UpdateValueAxis();
		UpdateLinearAxis();
		if (ChartView.TimeWindow == null)
		{
			UpdateDateTimeAxisRange();
		}

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

	public override void Unload()
	{
		IsVisible = false;
		UnloadModel();
	}

	private void AddAxis()
	{
		if (UseDateTimeAxis)
		{
			AddDateTimeAxis(ChartView.TimeWindow);
			AddNowTime();
			if (ChartView.ShowTimeTracker)
				AddTrackerLine();
		}
		else
		{
			AddLinearAxis();
		}

		AddMouseListeners();

		if (ChartView.Series.Count > 0 && ChartView.IsStacked)
			AddCategoryAxis();
		else
			AddValueAxis();
	}

	private void AddLinearAxis()
	{
		LinearAxis = new OxyPlot.Axes.LinearAxis
		{
			Position = AxisPosition.Bottom,
			MajorGridlineStyle = LineStyle.Solid,
			MajorGridlineColor = GridLineOxyColor,
			MinorGridlineStyle = LineStyle.None,
			MinorTickSize = 0,
			TitleColor = TextOxyColor,
			TextColor = TextOxyColor,
			TicklineColor = GridLineOxyColor,
			AxislineThickness = 0,
		};
		PlotModel!.Axes.Add(LinearAxis);
	}

	public OxyPlot.Axes.DateTimeAxis AddDateTimeAxis(TimeWindow? timeWindow = null)
	{
		DateTimeAxis = new OxyPlot.Axes.DateTimeAxis
		{
			Position = AxisPosition.Bottom,
			IntervalType = DateTimeIntervalType.Hours,
			MajorGridlineStyle = LineStyle.Solid,
			MajorGridlineColor = GridLineOxyColor,
			MinorGridlineStyle = LineStyle.None,
			MinorTickSize = 0,
			IntervalLength = 75,
			IsPanEnabled = false,
			AxislineStyle = LineStyle.None,
			AxislineThickness = 0,
			TickStyle = TickStyle.Outside,
			TicklineColor = GridLineOxyColor,
			AxisTickToLabelDistance = 2,
			TitleColor = TextOxyColor,
			TextColor = TextOxyColor,
		};

		if (timeWindow != null)
		{
			UpdateDateTimeAxis(timeWindow);
		}

		PlotModel!.Axes.Add(DateTimeAxis);
		return DateTimeAxis;
	}

	public OxyPlot.Axes.Axis AddValueAxis(AxisPosition axisPosition = AxisPosition.Left, string? key = null)
	{
		if (ChartView.LogBase is double logBase)
		{
			ValueAxis = new OxyPlot.Axes.LogarithmicAxis()
			{
				Base = logBase,
			};
		}
		else
		{
			ValueAxis = new OxyPlot.Axes.LinearAxis()
			{
				IntervalLength = 25,
			};
		}

		ValueAxis.Position = axisPosition;
		ValueAxis.MajorGridlineStyle = LineStyle.Solid;
		ValueAxis.MajorGridlineColor = GridLineOxyColor;
		ValueAxis.MinorGridlineStyle = LineStyle.None;
		ValueAxis.MinorTickSize = 0;
		ValueAxis.IsPanEnabled = false;
		ValueAxis.AxislineColor = GridLineOxyColor;
		ValueAxis.AxislineStyle = LineStyle.None;
		ValueAxis.AxislineThickness = 0;
		ValueAxis.TickStyle = TickStyle.Outside;
		ValueAxis.TicklineColor = GridLineOxyColor;
		ValueAxis.TitleColor = TextOxyColor;
		ValueAxis.TextColor = TextOxyColor;
		ValueAxis.LabelFormatter = NumberExtensions.FormattedShortDecimal;

		if (key != null)
		{
			ValueAxis.Key = key;
		}
		PlotModel!.Axes.Add(ValueAxis);
		return ValueAxis;
	}

	public OxyPlot.Axes.CategoryAxis AddCategoryAxis(AxisPosition axisPosition = AxisPosition.Left, string? key = null)
	{
		CategoryAxis = new OxyPlot.Axes.CategoryAxis
		{
			Position = axisPosition,
			IntervalLength = 20,
			MajorGridlineStyle = LineStyle.Solid,
			MajorGridlineColor = GridLineOxyColor,
			MinorGridlineStyle = LineStyle.None,
			MinorTickSize = 0,
			MinorStep = 20,
			MinimumMinorStep = 10,
			IsPanEnabled = false,
			AxislineColor = GridLineOxyColor,
			AxislineStyle = LineStyle.Solid,
			AxislineThickness = 0,
			TickStyle = TickStyle.Outside,
			TicklineColor = GridLineOxyColor,
			TitleColor = TextOxyColor,
			TextColor = TextOxyColor,
			LabelFormatter = NumberExtensions.FormattedShortDecimal,
		};
		if (key != null)
		{
			CategoryAxis.Key = key;
		}

		foreach (ListSeries listSeries in ChartView.Series)
		{
			CategoryAxis.Labels.Add(listSeries.Name);
		}

		PlotModel!.Axes.Add(CategoryAxis);
		return CategoryAxis;
	}

	private void AddMouseListeners()
	{
		PlotModel!.MouseDown += PlotModel_MouseDown;
		PlotModel.MouseMove += PlotModel_MouseMove;
		PlotModel.MouseUp += PlotModel_MouseUp;
		PlotModel.MouseLeave += PlotModel_MouseLeave;
	}

	public void AddSeries(ListSeries listSeries)
	{
		//if (listSeries.IsStacked)
		//	AddBarSeries(listSeries);
		//else
		AddListSeries(listSeries);
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

	public OxyPlot.Series.LineSeries? AddListSeries(ListSeries listSeries)
	{
		if (ChartSeries.Count >= SeriesLimit) return null;

		Color color =
			listSeries.Color?.AsAvaloniaColor() ??
			GetSeriesInfo(listSeries)?.Color ??
			GetColor(IdxSeriesInfo.Count);

		var lineSeries = new OxyPlotLineSeries(this, listSeries, UseDateTimeAxis)
		{
			Color = color.ToOxyColor(),
		};

		PlotModel!.Series.Insert(0, lineSeries);

		var oxyListSeries = new OxyPlotChartSeries(listSeries, lineSeries, color);

		lineSeries.MouseDown += (s, e) =>
		{
			OnSelectionChanged(new SeriesSelectedEventArgs(new List<ListSeries>() { listSeries }));
			Legend.SelectSeries(oxyListSeries.LineSeries, listSeries);
			e.Handled = true;
		};

		lineSeries.MouseUp += (s, e) =>
		{
			e.Handled = true; // Handle so zooming doesn't use?
		};

		ChartSeries.Add(oxyListSeries);
		IdxListToListSeries[listSeries.List] = listSeries;
		if (listSeries.Name != null)
			IdxNameToChartSeries[listSeries.Name] = oxyListSeries;
		UpdateSeriesInfo(oxyListSeries);
		return lineSeries;
	}

	/*private void AddNowTime()
	{
		var now = DateTime.UtcNow;
		if (ChartView.TimeWindow != null && ChartView.TimeWindow.EndTime < now.AddMinutes(1))
			return;

		var annotation = new OxyPlot.Annotations.LineAnnotation
		{
			Type = LineAnnotationType.Vertical,
			X = OxyPlot.Axes.DateTimeAxis.ToDouble(now.ToUniversalTime()),
			Color = NowColor,
			// LineStyle = LineStyle.Dot, // doesn't work for vertical?
		};

		PlotModel!.Annotations.Add(annotation);
	}*/

	private void AddTrackerLine()
	{
		_trackerAnnotation = new OxyPlot.Annotations.LineAnnotation
		{
			Type = LineAnnotationType.Vertical,
			Color = TimeTrackerOxyColor,
		};

		PlotModel!.Annotations.Add(_trackerAnnotation);
	}

	public override void AddAnnotation(ChartAnnotation chartAnnotation)
	{
		base.AddAnnotation(chartAnnotation);

		var oxyColor = chartAnnotation.Color!.Value.ToOxyColor();
		var annotationThreshold = new OxyPlot.Annotations.LineAnnotation
		{
			Text = chartAnnotation.Text,
			Type = chartAnnotation.Horizontal ? LineAnnotationType.Horizontal : LineAnnotationType.Vertical,
			X = chartAnnotation.X ?? 0,
			Y = chartAnnotation.Y ?? 0,
			Color = oxyColor,
			TextColor = oxyColor,
			StrokeThickness = chartAnnotation.StrokeThickness,
			LineStyle = LineStyle.Dot,
		};
		PlotModel!.Annotations.Add(annotationThreshold);
		//UpdateValueAxis();
	}

	private void UpdateLinearAxis()
	{
		if (LinearAxis == null)
			return;

		var (minimum, maximum, hasFraction) = GetXValueRange();

		if (minimum == double.MaxValue)
		{
			// Didn't find any values
			minimum = 0;
			maximum = 1;
		}

		if (!hasFraction)
		{
			LinearAxis.MinimumMajorStep = 1;
		}

		LinearAxis.Minimum = minimum;
		LinearAxis.Maximum = maximum;
	}

	private void UpdateDateTimeAxis(TimeWindow? timeWindow)
	{
		if (timeWindow == null)
		{
			DateTimeAxis!.Minimum = double.NaN;
			DateTimeAxis.Maximum = double.NaN;
			DateTimeAxis.IntervalLength = 75;
			DateTimeAxis.StringFormat = null;
			//UpdateDateTimeInterval(timeWindow.Duration);
		}
		else
		{
			DateTimeAxis!.Minimum = OxyPlot.Axes.DateTimeAxis.ToDouble(timeWindow.StartTime);
			DateTimeAxis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(timeWindow.EndTime);
			//DateTimeAxis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(endTime.AddSeconds(duration / 25.0)); // labels get clipped without this
			UpdateDateTimeInterval(timeWindow.Duration);
		}
	}

	private void UpdateDateTimeInterval(TimeSpan windowDuration)
	{
		var dateFormat = DateTimeFormat.GetWindowFormat(windowDuration)!;
		TimeSpan stepDuration = windowDuration.PeriodDuration(10).Max(dateFormat.Minimum);
		DateTimeAxis!.StringFormat = dateFormat.TextFormat;

		DateTimeAxis.MinimumMajorStep = stepDuration.TotalDays;

		double widthPerLabel = 6 * DateTimeAxis.StringFormat.Length + 25;
		DateTimeAxis.IntervalLength = Math.Max(50, widthPerLabel);
	}

	private void UpdateDateTimeAxisRange()
	{
		if (DateTimeAxis == null)
			return;

		var (minimum, maximum, hasFraction) = GetXValueRange();

		if (minimum != double.MaxValue)
		{
			DateTimeAxis.Minimum = minimum;
			DateTimeAxis.Maximum = maximum;
		}

		if (ChartView.TimeWindow == null)
		{
			DateTime startTime = OxyPlot.Axes.DateTimeAxis.ToDateTime(DateTimeAxis.Minimum);
			DateTime endTime = OxyPlot.Axes.DateTimeAxis.ToDateTime(DateTimeAxis.Maximum);

			ChartView.TimeWindow = new TimeWindow(startTime, endTime).Trim();

			UpdateDateTimeAxis(ChartView.TimeWindow);
		}

		//UpdateDateTimeInterval(double totalSeconds);
	}

	private (double minimum, double maximum, bool hasFraction) GetXValueRange()
	{
		double minimum = double.MaxValue;
		double maximum = double.MinValue;
		bool hasFraction = false;

		foreach (OxyPlot.Series.Series series in PlotModel!.Series)
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
		return (minimum, maximum, hasFraction);
	}

	private (double minimum, double maximum, bool hasFraction) GetYValueRange()
	{
		double minimum = double.MaxValue;
		double maximum = double.MinValue;
		bool hasFraction = false;

		foreach (OxyPlot.Series.Series series in PlotModel!.Series)
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

					hasFraction |= y % 1 != 0.0;

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

				PropertyInfo? propertyInfo = null;
				foreach (var item in scatterSeries.ItemsSource)
				{
					if (propertyInfo == null)
					{
						propertyInfo = item.GetType().GetProperty(scatterSeries.DataFieldY);
					}

					var value = propertyInfo!.GetValue(item);
					double d = Convert.ToDouble(value);
					if (double.IsNaN(d))
						continue;

					minimum = Math.Min(minimum, d);
					maximum = Math.Max(maximum, d);
				}
			}
		}
		return (minimum, maximum, hasFraction);
	}

	public void UpdateValueAxis() // OxyPlot.Axes.LinearAxis valueAxis, string axisKey = null
	{
		if (ValueAxis == null)
			return;

		var (minimum, maximum, hasFraction) = GetYValueRange();

		if (minimum == double.MaxValue)
		{
			// didn't find any values
			minimum = 0;
			maximum = 1;
		}

		foreach (OxyPlot.Annotations.Annotation annotation in PlotModel!.Annotations)
		{
			if (annotation is OxyPlot.Annotations.LineAnnotation lineAnnotation)
				maximum = Math.Max(lineAnnotation.Y * 1.1, maximum);
		}

		ValueAxis.MinimumMajorStep = hasFraction ? 0 : 1;

		double? minValue = ChartView.MinValue;
		if (minValue != null)
			minimum = minValue.Value;

		if (ChartView.LogBase != null)
		{
			ValueAxis.Minimum = minimum * 0.85;
			ValueAxis.Maximum = maximum * 1.15;
		}
		else
		{
			var margin = (maximum - minimum) * MarginPercent;
			if (minimum == maximum)
				margin = Math.Abs(minimum);

			if (margin == 0)
				margin = 1;

			if (minValue != null)
				ValueAxis.Minimum = Math.Max(minimum - margin, minValue.Value - Math.Abs(margin) * 0.05);
			else
				ValueAxis.Minimum = minimum - margin;
			ValueAxis.Maximum = maximum + margin;
		}
	}

	public override void InvalidateChart()
	{
		Dispatcher.UIThread.InvokeAsync(() => PlotView?.Model?.InvalidatePlot(true), DispatcherPriority.Background);
	}

	private void UnloadModel()
	{
		PlotView.Model = null;
		LinearAxis = null;
		DateTimeAxis = null;

		Legend?.Unload();

		ClearSeries();
	}

	private void ClearSeries()
	{
		PlotModel?.Series.Clear();

		ChartSeries.Clear();
		IdxListToListSeries.Clear();
		IdxNameToChartSeries.Clear();

		/*foreach (ListSeries listSeries in ChartSeries)
		{
			if (listSeries.iList is INotifyCollectionChanged iNotifyCollectionChanged)
				iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;
		}*/
	}

	private void ClearListeners()
	{
		if (Legend != null)
		{
			Legend.OnSelectionChanged -= Legend_OnSelectionChanged;
			Legend.OnVisibleChanged -= Legend_OnVisibleChanged;
		}

		PlotView.EffectiveViewportChanged -= Chart_EffectiveViewPortChanged;

		OnMouseCursorChanged -= TabControlOxyPlot_OnMouseCursorChanged;
	}

	private void ZoomIn()
	{
		double left = Math.Min(_startDataPoint!.Value.X, _endDataPoint!.Value.X);
		double right = Math.Max(_startDataPoint.Value.X, _endDataPoint!.Value.X);

		if (double.IsNaN(DateTimeAxis!.Minimum))
		{
			UpdateDateTimeAxisRange();
		}

		DateTimeAxis.Minimum = Math.Max(left, DateTimeAxis.Minimum);
		DateTimeAxis.Maximum = Math.Min(right, DateTimeAxis.Maximum);

		DateTime startTime = OxyPlot.Axes.DateTimeAxis.ToDateTime(DateTimeAxis.Minimum);
		DateTime endTime = OxyPlot.Axes.DateTimeAxis.ToDateTime(DateTimeAxis.Maximum);
		var timeWindow = new TimeWindow(startTime, endTime).Trim();

		UpdateDateTimeAxis(timeWindow);
		if (ChartView.TimeWindow != null)
		{
			ChartView.TimeWindow.Select(timeWindow);
		}
		else
		{
			UpdateTimeWindow(timeWindow);
		}
	}

	private void ZoomOut()
	{
		if (ChartView.TimeWindow != null)
		{
			UpdateDateTimeAxis(ChartView.TimeWindow);
			ChartView.TimeWindow.Select(null);
		}
		else
		{
			UpdateTimeWindow(null);
		}
	}

	private void ListGroup_OnTimesChanged(object? sender, TimeWindowEventArgs e)
	{
		UpdateTimeWindow(e.TimeWindow);
	}

	private void UpdateTimeWindow(TimeWindow? timeWindow)
	{
		UpdateDateTimeAxis(timeWindow);
		UpdateValueAxis();

		ChartView.SortByTotal();
		Legend.RefreshModel();

		PlotView!.InvalidatePlot(true);
		PlotView.Model.InvalidatePlot(true);
	}

	// Hide slow controls when not viewable
	private void Chart_EffectiveViewPortChanged(object? sender, EffectiveViewportChangedEventArgs e)
	{
		UpdateVisible();
	}

	private void UpdateVisible()
	{
		if (PlotView == null || !IsLoaded) return;

		bool visible = AvaloniaUtils.IsControlVisible(this);
		if (visible != PlotView.IsVisible)
		{
			PlotView.IsVisible = visible;
			Legend.IsVisible = visible;
			PlotView.InvalidateArrange();
			Legend.InvalidateArrange();
			//PlotModel.InvalidatePlot(false);
		}
	}

	private void StopSelecting()
	{
		if (_zoomAnnotation != null)
		{
			PlotModel!.Annotations.Remove(_zoomAnnotation);
		}
		_selecting = false;
	}

	private void UpdateMouseSelection(DataPoint endDataPoint)
	{
		_zoomAnnotation ??= new OxyPlot.Annotations.RectangleAnnotation()
		{
			Fill = OxyColor.FromAColor((byte)AtlasTheme.ChartBackgroundSelectedAlpha, AtlasTheme.ChartBackgroundSelected.ToOxyColor()),
			Stroke = OxyColor.FromAColor((byte)180, AtlasTheme.ChartBackgroundSelected.ToOxyColor()),
			StrokeThickness = 1,
		};

		try
		{
			if (!PlotModel!.Annotations.Contains(_zoomAnnotation))
			{
				PlotModel.Annotations.Add(_zoomAnnotation);
			}
		}
		catch (Exception)
		{
		}

		_zoomAnnotation.MinimumX = Math.Min(_startDataPoint!.Value.X, endDataPoint.X);
		_zoomAnnotation.MaximumX = Math.Max(_startDataPoint.Value.X, endDataPoint.X);

		_zoomAnnotation.MinimumY = ValueAxis!.Minimum;
		_zoomAnnotation.MaximumY = ValueAxis.Maximum;

		Dispatcher.UIThread.Post(() => PlotModel!.InvalidatePlot(false), DispatcherPriority.Background);
	}

	private void PlotModel_MouseDown(object? sender, OxyMouseDownEventArgs e)
	{
		if (!_selecting || _startDataPoint == null)
		{
			_startDataPoint = OxyPlot.Axes.Axis.InverseTransform(e.Position, XAxis, ValueAxis);
			_startScreenPoint = e.Position;
			_selecting = true;
			e.Handled = true;
		}
	}

	private void PlotModel_MouseMove(object? sender, OxyMouseEventArgs e)
	{
		DataPoint dataPoint = OxyPlot.Axes.Axis.InverseTransform(e.Position, XAxis, ValueAxis);
		var moveEvent = new MouseCursorMovedEventArgs(dataPoint.X);
		_mouseCursorChangedEventSource?.Raise(sender, moveEvent);

		if (_selecting && _startDataPoint != null)
		{
			_endDataPoint = OxyPlot.Axes.Axis.InverseTransform(e.Position, XAxis, ValueAxis);
			UpdateMouseSelection(_endDataPoint.Value);
		}
	}

	private void PlotModel_MouseUp(object? sender, OxyMouseEventArgs e)
	{
		if (_selecting && _startDataPoint != null)
		{
			_endDataPoint = OxyPlot.Axes.Axis.InverseTransform(e.Position, XAxis, ValueAxis);
			double width = Math.Abs(e.Position.X - _startScreenPoint.X);
			if (width > MinSelectionWidth)
			{
				ZoomIn();
			}
			else if (ChartView.TimeWindow?.Selection != null)
			{
				ZoomOut();
			}
			else
			{
				// Show all series
				Legend.SetAllVisible(true, true);
			}
			StopSelecting();
		}
	}

	// Hide cursor when out of scope
	private void PlotModel_MouseLeave(object? sender, OxyMouseEventArgs e)
	{
		var moveEvent = new MouseCursorMovedEventArgs(0);
		_mouseCursorChangedEventSource?.Raise(sender, moveEvent);
	}

	// Update mouse tracker
	private void TabControlOxyPlot_OnMouseCursorChanged(object? sender, MouseCursorMovedEventArgs e)
	{
		if (sender == PlotView?.Controller || _trackerAnnotation == null)
			return;

		_trackerAnnotation.X = e.X;
		Dispatcher.UIThread.Post(() => PlotModel!.InvalidatePlot(false), DispatcherPriority.Background);
	}

	private void PlotView_KeyDown(object? sender, KeyEventArgs e)
	{
		// These keys are used for navigating in the TabViewer
		if (e.Key == Key.Left || e.Key == Key.Right)
		{
			RaiseEvent(e);
		}
	}

	private void PlotView_PointerExited(object? sender, PointerEventArgs e)
	{
		if (HoverSeries != null)
		{
			HoverSeries = null;
			Legend.UnhighlightAll(true);
		}
	}

	private void TitleTextBlock_PointerEntered(object? sender, PointerEventArgs e)
	{
		if (IsTitleSelectable)
		{
			TitleTextBlock!.Foreground = AtlasTheme.GridBackgroundSelected;
		}
	}

	private void TitleTextBlock_PointerExited(object? sender, PointerEventArgs e)
	{
		TitleTextBlock!.Foreground = AtlasTheme.BackgroundText;
	}

	private void Legend_OnSelectionChanged(object? sender, EventArgs e)
	{
		StopSelecting();
		UpdateValueAxis();
		OnSelectionChanged(new SeriesSelectedEventArgs(SelectedSeries));
	}

	private void Legend_OnVisibleChanged(object? sender, EventArgs e)
	{
		UpdateValueAxis();
	}

	public void Dispose()
	{
		ClearListeners();
		UnloadModel();
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
}
