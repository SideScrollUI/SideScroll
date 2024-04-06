using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Themes;
using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Skia;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Diagnostics;

namespace Atlas.UI.Avalonia.Charts.LiveCharts;

public class LiveChartCreator : IControlCreator
{
	public static void Register()
	{
		TabView.ControlCreators[typeof(ChartView)] = new LiveChartCreator();
	}

	public void AddControl(TabInstance tabInstance, TabControlSplitContainer container, object obj)
	{
		var chartView = (ChartView)obj;

		var tabChart = new TabControlLiveChart(tabInstance, chartView, true);

		container.AddControl(tabChart, true, SeparatorType.Spacer);
	}
}

public class TabControlLiveChart : TabControlChart<ISeries>, IDisposable
{
	public static SKColor TimeTrackerSkColor { get; set; } = TimeTrackerColor.ToSKColor();
	public static SKColor GridLineSkColor { get; set; } = GridLineColor.ToSKColor();
	public static SKColor TextSkColor { get; set; } = TextColor.ToSKColor();
	public static SKColor TooltipBackgroundColor { get; set; } = SKColor.Parse("#102670").WithAlpha(225);

	public CartesianChart Chart;

	public TabControlChartLegend<ISeries> Legend;

	public Axis XAxis { get; set; }
	public Axis YAxis { get; set; } // left/right?

	public List<LiveChartSeries> LiveChartSeries { get; private set; } = new();

	public ChartSeries<ISeries>? HoverSeries;

	private List<RectangularSection> _sections = new();
	private RectangularSection? _trackerSection;
	private RectangularSection? _zoomSection;

	public Point? CursorPosition;

	private ChartPoint? _pointClicked;

	private bool _selecting;
	private Point _startScreenPoint;
	private LvcPointD? _startDataPoint;
	private LvcPointD? _endDataPoint;

	public int MaxFindDistance = 20;

	public TabControlLiveChart(TabInstance tabInstance, ChartView chartView, bool fillHeight = false) : 
		base(tabInstance, chartView, fillHeight)
	{
		ColumnDefinitions = new ColumnDefinitions("*,Auto");
		RowDefinitions = new RowDefinitions("Auto,*,Auto");

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		Background = AtlasTheme.TabBackground; // Grid lines look bad when hovering without this

		XAxis = CreateXAxis();
		YAxis = CreateYAxis();

		Chart = new CartesianChart()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			XAxes = new List<Axis> { XAxis },
			YAxes = new List<Axis> { YAxis },
			TooltipBackgroundPaint = new SolidColorPaint(TooltipBackgroundColor),
			TooltipTextPaint = new SolidColorPaint(AtlasTheme.TitleForeground.Color.AsSkColor()),
			TooltipFindingStrategy = TooltipFindingStrategy.CompareAllTakeClosest,
			Tooltip = new LiveChartTooltip(this),
			LegendPosition = LegendPosition.Hidden,
			AnimationsSpeed = TimeSpan.Zero,
			MinWidth = 150,
			MinHeight = 140,
			[Grid.RowProperty] = 1,
		};

		Chart.ChartPointPointerDown += Chart_ChartPointPointerDown;
		Chart.PointerExited += Chart_PointerExited;
		Chart.PointerPressed += TabControlLiveChart_PointerPressed;
		Chart.PointerReleased += TabControlLiveChart_PointerReleased;
		Chart.PointerMoved += TabControlLiveChart_PointerMoved;
		Chart.EffectiveViewportChanged += Chart_EffectiveViewportChanged;

		ReloadView();

		if (TitleTextBlock != null)
		{
			Children.Add(TitleTextBlock);
		}
		else
		{
			RowDefinitions[0].Height = new GridLength(0);
		}

		Children.Add(Chart);

		Legend = new TabControlLiveChartLegend(this);
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
		else
		{
			Legend.IsVisible = false;
		}
		Children.Add(Legend);
		Legend.OnSelectionChanged += Legend_OnSelectionChanged;
		//Legend.OnVisibleChanged += Legend_OnVisibleChanged;

		OnMouseCursorChanged += TabControlChart_OnMouseCursorChanged;
		if (ChartView.TimeWindow != null)
		{
			ChartView.TimeWindow.OnSelectionChanged += TimeWindow_OnSelectionChanged;
		}

		if (UseDateTimeAxis)
		{
			AddNowTime();
		}
		AddSections();
	}

	public void LoadView(ChartView chartView)
	{
		ClearSeries();
		ChartView = chartView;
		ReloadView();
	}

	// Reuses previous colors and TimeWindow
	public override void UpdateView(ChartView chartView)
	{
		ClearSeries();

		ChartView.Series = chartView.Series;
		ChartView.TimeWindow = chartView.TimeWindow ?? ChartView.TimeWindow;

		ReloadView();
	}

	public override void ReloadView()
	{
		ChartView.SortByTotal();

		Chart.Series = ChartView.Series
			.Take(SeriesLimit)
			.Select(s => AddListSeries(s))
			.ToList();

		UpdateAxis();

		Legend?.RefreshModel();

		IsVisible = true;
	}

	public void Refresh()
	{
		UpdateAxis();

		Legend.RefreshModel();

		//InvalidateChart();
	}

	private static Axis CreateXAxis()
	{
		return new Axis
		{
			ShowSeparatorLines = true,
			SeparatorsPaint = new SolidColorPaint(GridLineSkColor),
			LabelsPaint = new SolidColorPaint(TextSkColor),
			TextSize = 14,
		};
	}

	public Axis CreateYAxis() // AxisPosition axisPosition = AxisPosition.Left)
	{
		Axis axis;
		if (ChartView.LogBase is double logBase)
		{
			axis = new LogaritmicAxis(logBase)
			{
				Labeler = (v) => Math.Pow(logBase, v).FormattedShortDecimal(),
			};
		}
		else
		{
			axis = new Axis()
			{
				Labeler = NumberExtensions.FormattedShortDecimal,
			};
		}

		axis.Padding = new Padding(10, 2);
		axis.SeparatorsPaint = new SolidColorPaint(GridLineSkColor);
		axis.LabelsPaint = new SolidColorPaint(TextSkColor);
		axis.TextSize = 14;

		return axis;

		/*axis.Name = "Amount";
		axis.NamePadding = new Padding(0, 15);
		axis.LabelsPaint = new SolidColorPaint
		{
			Color = SKColors.Blue,
			FontFamily = "Times New Roman",
			SKFontStyle = new SKFontStyle(SKFontStyleWeight.ExtraBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic)
		};
		axis.Position = axisPosition;*/
	}

	public ISeries AddListSeries(ListSeries listSeries)
	{
		Color color = 
			listSeries.Color?.AsAvaloniaColor() ??
			GetSeriesInfo(listSeries)?.Color ??
			GetColor(IdxSeriesInfo.Count);

		var liveChartSeries = new LiveChartSeries(this, listSeries, color, UseDateTimeAxis);
		XAxisPropertyInfo = listSeries.XPropertyInfo;

		var chartSeries = new ChartSeries<ISeries>(listSeries, liveChartSeries.LineSeries, color);
		LiveChartSeries.Add(liveChartSeries);
		ChartSeries.Add(chartSeries);
		IdxListToListSeries[listSeries.List] = listSeries;
		if (listSeries.Name != null)
		{
			IdxNameToChartSeries[listSeries.Name] = chartSeries;
		}
		UpdateSeriesInfo(chartSeries);
		return liveChartSeries.LineSeries;
	}

	private void AddSections()
	{
		Annotations.AddRange(ChartView.Annotations);
		_sections = ChartView.Annotations
			.Select(a => CreateAnnotation(a))
			.ToList();

		if (Annotations.Count > 0)
		{
			UpdateYAxis();
		}

		if (UseDateTimeAxis)
		{
			if (ChartView.ShowTimeTracker)
			{
				_sections.Add(CreateTrackerLine());
			}

			var skColor = AtlasTheme.ChartBackgroundSelected.Color.AsSkColor();
			_zoomSection = new RectangularSection
			{
				Label = "",
				Stroke = new SolidColorPaint(skColor.WithAlpha(180)),
				Fill = new SolidColorPaint(skColor.WithAlpha((byte)AtlasTheme.ChartBackgroundSelectedAlpha)),
				IsVisible = false,
			};
			_sections.Add(_zoomSection);
		}

		Chart.Sections = _sections;
	}

	private RectangularSection CreateTrackerLine()
	{
		_trackerSection = new RectangularSection
		{
			Label = "",
			Stroke = new SolidColorPaint(TimeTrackerSkColor),
			IsVisible = false,
		};
		return _trackerSection;
	}

	public override void AddAnnotation(ChartAnnotation chartAnnotation)
	{
		base.AddAnnotation(chartAnnotation);

		_sections.Add(CreateAnnotation(chartAnnotation));

		UpdateYAxis();
	}

	public RectangularSection CreateAnnotation(ChartAnnotation chartAnnotation)
	{
		var c = chartAnnotation.Color!.Value;
		var color = new SKColor(c.R, c.G, c.B, c.A);
		var section = new RectangularSection
		{
			Label = chartAnnotation.Text ?? "",
			LabelSize = 14,
			LabelPaint = new SolidColorPaint(color.WithAlpha(220))
			{
				SKTypeface = SKTypeface.FromFamilyName("Inter", SKFontStyle.Bold),
			},
			Stroke = new SolidColorPaint(color.WithAlpha(200), (float)chartAnnotation.StrokeThickness),
		};

		if (chartAnnotation.X is double x)
		{
			section.Xj = x;
			section.Xi = x;
		}

		if (chartAnnotation.Y is double y)
		{
			if (ChartView.LogBase is double logBase)
			{
				y = Math.Log(y, logBase);
			}
			section.Yj = y;
			section.Yi = y;
		}

		return section;
	}

	public override void InvalidateChart()
	{
		Dispatcher.UIThread.Post(Chart!.InvalidateVisual, DispatcherPriority.Background);
	}

	public void UpdateAxis()
	{
		UpdateLinearAxis();
		UpdateDateTimeAxis();
		UpdateYAxis();
	}

	private void UpdateLinearAxis()
	{
		if (XAxis == null || UseDateTimeAxis)
			return;

		var (minimum, maximum, hasFraction) = GetXValueRange();

		if (!hasFraction)
		{
			XAxis.MinStep = 1;
		}

		/*
		if (minimum == double.MaxValue)
		{
			// didn't find any values
			minimum = 0;
			maximum = 1;
		}

		XAxis.MinLimit = minimum;
		XAxis.MaxLimit = maximum;*/
	}

	private void UpdateDateTimeAxis()
	{
		if (XAxis == null || !UseDateTimeAxis)
			return;

		var (minimum, maximum, hasFraction) = GetXValueRange();

		/*if (minimum != double.MaxValue)
		{
			XAxis.MinLimit = minimum;
			XAxis.MaxLimit = maximum;
		}*/

		if (ChartView.TimeWindow == null && minimum != double.MaxValue)
		{
			var startTime = new DateTime((long)minimum, DateTimeKind.Utc);
			var endTime = new DateTime((long)maximum, DateTimeKind.Utc);

			ChartView.TimeWindow = new TimeWindow(startTime, endTime).Trim();
		}

		UpdateDateTimeAxisWindow(ChartView.TimeWindow?.Selection ?? ChartView.TimeWindow);
	}

	private void UpdateDateTimeAxisWindow(TimeWindow? timeWindow)
	{
		if (timeWindow == null)
		{
			XAxis.MinLimit = null;
			XAxis.MaxLimit = null;
			//UpdateDateTimeInterval(timeWindow.Duration);
		}
		else
		{
			XAxis.MinLimit = timeWindow.StartTime.Ticks;
			XAxis.MaxLimit = timeWindow.EndTime.Ticks;
			UpdateDateTimeInterval(timeWindow.Duration);
		}
	}

	private void UpdateDateTimeInterval(TimeSpan windowDuration)
	{
		var dateFormat = DateTimeFormat.GetWindowFormat(windowDuration)!;
		TimeSpan stepDuration = windowDuration.PeriodDuration(8).Max(dateFormat.Minimum);

		XAxis.Labeler = value => new DateTime((long)value, DateTimeKind.Utc).ToString(dateFormat.TextFormat);
		XAxis.UnitWidth = stepDuration.Ticks; // Hover depends on this
		XAxis.MinStep = stepDuration.Ticks;
	}

	private void UpdateTimeWindow(TimeWindow? timeWindow)
	{
		UpdateDateTimeAxisWindow(timeWindow);
		//UpdateYAxis();

		ChartView.SortByTotal();
		Legend.RefreshModel();

		//InvalidateChart();
	}

	public void UpdateYAxis() // Axis yAxis, string axisKey = null
	{
		if (YAxis == null) return;

		var (minimum, maximum, hasFraction) = GetYValueRange();

		if (minimum == double.MaxValue)
		{
			// Didn't find any values
			minimum = 0;
			maximum = 1;
		}
		else
		{
			double difference = maximum - minimum;
			if (difference > 10 || (difference != 0 && hasFraction))
			{
				YAxis.UnitWidth = (difference * 0.2).RoundToSignificantFigures(1);
			}
		}

		foreach (var annotation in Annotations)
		{
			if (annotation.Y != null)
			{
				maximum = Math.Max(annotation.Y.Value * 1.1, maximum);
			}
		}

		YAxis.MinStep = hasFraction ? 0 : 1;

		double? minValue = ChartView.MinValue;
		if (minValue != null)
			minimum = minValue.Value;

		if (ChartView.LogBase is double logBase)
		{
			// Log 0 can return infinity, which is difficult to render
			YAxis.MinLimit = Math.Max(double.MinValue, Math.Log(minimum, logBase) * 0.85);
			YAxis.MaxLimit = Math.Min(double.MaxValue, Math.Log(maximum, logBase) * 1.15);

			if (maximum - minimum > 10)
			{
				YAxis.MinStep = 1;
			}
		}
		else
		{
			var margin = (maximum - minimum) * MarginPercent;
			if (minimum == maximum)
				margin = Math.Abs(minimum);

			if (margin == 0)
				margin = 1;

			if (minValue != null)
			{
				YAxis.MinLimit = Math.Max(minimum - margin, minValue.Value - Math.Abs(margin) * 0.05);
			}
			else
			{
				YAxis.MinLimit = minimum - margin;
			}
			YAxis.MaxLimit = maximum + margin;
		}
	}

	private (double minimum, double maximum, bool hasFraction) GetXValueRange()
	{
		double minimum = double.MaxValue;
		double maximum = double.MinValue;
		bool hasFraction = false;

		foreach (ISeries series in Chart.Series)
		{
			if (series is LiveChartLineSeries lineSeries)
			{
				if (!lineSeries.IsVisible) continue;

				foreach (LiveChartPoint chartPoint in lineSeries.Values!)
				{
					double? x = chartPoint.X;
					if (x == null || double.IsNaN(x.Value))
						continue;

					minimum = Math.Min(minimum, x.Value);
					maximum = Math.Max(maximum, x.Value);

					hasFraction |= (x % 1 != 0.0);
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

		foreach (LiveChartSeries series in LiveChartSeries)
		{
			if (!series.LineSeries.IsVisible) continue;

			foreach (var dataPoint in series.LineSeries.Values!)
			{
				if (dataPoint.Y is double y && !double.IsNaN(y))
				{
					if (XAxis != null && (dataPoint.X < XAxis.MinLimit || dataPoint.X > XAxis.MaxLimit))
						continue;

					hasFraction |= (y % 1 != 0.0);

					minimum = Math.Min(minimum, y);
					maximum = Math.Max(maximum, y);
				}
			}
		}
		return (minimum, maximum, hasFraction);
	}

	public ChartPoint? FindClosestPoint(LvcPoint pointerPosition, double maxDistance)
	{
		return LiveChartSeries
			.Where(series => series.LineSeries.IsVisible)
			.SelectMany(s => s.LineSeries.Fetch(Chart.CoreChart))
			.Select(x => new { distance = LiveChartLineSeries.GetDistanceTo(x, pointerPosition), point = x })
			.Where(x => x.distance < maxDistance)
			.MinBy(x => x.distance)
			?.point;
	}

	public void SelectPoint(ChartPoint chartPoint)
	{
		if (IdxNameToChartSeries.TryGetValue(chartPoint.Context.Series.Name!, out var series))
		{
			OnSelectionChanged(new SeriesSelectedEventArgs(new List<ListSeries>() { series.ListSeries }));
			Legend.SelectSeries(series.LineSeries, series.ListSeries);
		}
	}

	private void UpdateZoomSection(LvcPointD endDataPoint)
	{
		if (_zoomSection == null || _startDataPoint == null) return;

		_zoomSection.Xi = Math.Min(_startDataPoint!.Value.X, endDataPoint.X);
		_zoomSection.Xj = Math.Max(_startDataPoint.Value.X, endDataPoint.X);

		UpdateAxis();
		InvalidateChart();
	}

	private void ZoomIn()
	{
		if (!UseDateTimeAxis) return;

		double left = Math.Min(_startDataPoint!.Value.X, _endDataPoint!.Value.X);
		double right = Math.Max(_startDataPoint.Value.X, _endDataPoint!.Value.X);

		if (XAxis.MinLimit == null || double.IsNaN(XAxis.MinLimit.Value))
		{
			UpdateDateTimeAxis();
		}

		XAxis.MinLimit = Math.Max(left, XAxis.MinLimit!.Value);
		XAxis.MaxLimit = Math.Min(right, XAxis.MaxLimit!.Value);

		var startTime = new DateTime((long)XAxis.MinLimit!.Value, DateTimeKind.Utc);
		var endTime = new DateTime((long)XAxis.MaxLimit.Value, DateTimeKind.Utc);
		var timeWindow = new TimeWindow(startTime, endTime).Trim();

		UpdateDateTimeAxisWindow(timeWindow);
		if (ChartView.TimeWindow != null)
		{
			ChartView.TimeWindow.Select(timeWindow);
		}
		else
		{
			UpdateTimeWindow(timeWindow);
		}
		UpdateYAxis();
	}

	private void ZoomOut()
	{
		if (ChartView.TimeWindow != null)
		{
			UpdateDateTimeAxisWindow(ChartView.TimeWindow);
			ChartView.TimeWindow.Select(null);
		}
		else
		{
			UpdateTimeWindow(null);
		}
		UpdateYAxis();
	}

	private void StopSelecting()
	{
		if (_zoomSection != null)
		{
			_zoomSection!.IsVisible = false;
		}
		_startDataPoint = null;
		_selecting = false;
	}

	private void TimeWindow_OnSelectionChanged(object? sender, TimeWindowEventArgs e)
	{
		UpdateTimeWindow(e.TimeWindow);
	}

	private void Legend_OnSelectionChanged(object? sender, EventArgs e)
	{
		StopSelecting();
		UpdateYAxis();
		OnSelectionChanged(new SeriesSelectedEventArgs(SelectedSeries));
	}

	// Update mouse tracker
	private void TabControlChart_OnMouseCursorChanged(object? sender, MouseCursorMovedEventArgs e)
	{
		if (_trackerSection == null) return;

		_trackerSection.Xi = e.X;
		_trackerSection.Xj = e.X;
		_trackerSection.IsVisible = true;

		InvalidateChart();
	}

	private void TabControlLiveChart_PointerMoved(object? sender, PointerEventArgs e)
	{
		// Store the mouse down point, check it when mouse button is released to determine if the context menu should be shown
		var point = e.GetPosition(Chart);
		CursorPosition = point;
		try
		{
			ChartPoint? hitPoint = FindClosestPoint(new LvcPoint(point.X, point.Y), MaxFindDistance);
			if (hitPoint != null)
			{
				if (hitPoint.Context.Series.Name is string name)
				{
					Legend.HighlightSeries(name);
					if (IdxNameToChartSeries.TryGetValue(name, out ChartSeries<ISeries>? series))
					{
						HoverSeries = series;
					}
				}
			}
			else
			{
				if (HoverSeries != null)
				{
					HoverSeries = null;
					Legend.UnhighlightAll(true);
				}
			}

			LvcPointD dataPoint = Chart!.ScalePixelsToData(new LvcPointD(point.X, point.Y));

			var moveEvent = new MouseCursorMovedEventArgs(dataPoint.X);
			_mouseCursorChangedEventSource?.Raise(sender, moveEvent);

			UpdateZoomSection(dataPoint);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
		}
	}

	private void Chart_ChartPointPointerDown(IChartView chart, ChartPoint? point)
	{
		_pointClicked = point;
	}

	private void TabControlLiveChart_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		var point = e.GetPosition(Chart);
		_startScreenPoint = point;
		_startDataPoint = Chart!.ScalePixelsToData(new LvcPointD(point.X, point.Y));

		if (!_selecting)
		{
			if (_zoomSection != null)
			{
				UpdateZoomSection(_startDataPoint!.Value);
				_zoomSection.IsVisible = true;
			}
			_selecting = true;
			e.Handled = true;
		}
	}

	private void TabControlLiveChart_PointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		if (_selecting && _startDataPoint != null)
		{
			var point = e.GetPosition(Chart);
			_endDataPoint = Chart!.ScalePixelsToData(new LvcPointD(point.X, point.Y));
			double width = Math.Abs(point.X - _startScreenPoint.X);
			if (width > MinSelectionWidth)
			{
				ZoomIn();
			}
			else if (_pointClicked != null)
			{
				SelectPoint(_pointClicked);
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
		else
		{
			// Show all series
			Legend.SetAllVisible(true, true);
		}
	}

	private void Chart_PointerExited(object? sender, PointerEventArgs e)
	{
		if (HoverSeries != null)
		{
			HoverSeries = null;
			Legend.UnhighlightAll(true);
		}

		// Hide cursor when out of scope
		var moveEvent = new MouseCursorMovedEventArgs(0);
		_mouseCursorChangedEventSource?.Raise(sender, moveEvent);
	}

	public override void Unload()
	{
		IsVisible = false;
		ClearSeries();
	}

	private void ClearSeries()
	{
		Legend?.Unload();

		ChartSeries.Clear();
		LiveChartSeries.Clear();
		IdxListToListSeries.Clear();
		IdxNameToChartSeries.Clear();
	}

	private void ClearListeners()
	{
		if (Legend != null)
		{
			Legend.OnSelectionChanged -= Legend_OnSelectionChanged;
			//Legend.OnVisibleChanged -= Legend_OnVisibleChanged;
		}

		Chart.PointerPressed -= TabControlLiveChart_PointerPressed;
		Chart.PointerReleased -= TabControlLiveChart_PointerReleased;
		Chart.PointerMoved -= TabControlLiveChart_PointerMoved;
		Chart.ChartPointPointerDown -= Chart_ChartPointPointerDown;
		Chart.PointerExited -= Chart_PointerExited;
		OnMouseCursorChanged -= TabControlChart_OnMouseCursorChanged;
		Chart.EffectiveViewportChanged -= Chart_EffectiveViewportChanged;

		if (ChartView.TimeWindow != null)
		{
			ChartView.TimeWindow.OnSelectionChanged -= TimeWindow_OnSelectionChanged;
		}
	}

	public void Dispose()
	{
		Children.Clear();
		ClearListeners();
		ClearSeries();
	}

	private void Chart_EffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
	{
		UpdateVisible();
	}

	// Hide slow controls when not viewable
	private void UpdateVisible()
	{
		if (Chart == null || !IsLoaded) return;

		bool visible = AvaloniaUtils.IsControlVisible(this);
		if (visible != Chart.IsVisible)
		{
			Chart.IsVisible = visible;
			Legend.IsVisible = visible;
			Legend.InvalidateArrange();
			//InvalidateChart();
		}
	}

	/*private void Legend_OnVisibleChanged(object? sender, EventArgs e)
	{
		UpdateYAxis();
	}

	private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		lock (Chart.SyncContext)
		{
			//Update();
			int index = ListToTabIndex[(IList)sender];
			ListSeries listSeries = ListToTabSeries[(IList)sender];
			AddPoints((LineSeries)plotModel.Series[index], listSeries, e.NewItems);
		}

		InvalidateChart();
	}*/
}
