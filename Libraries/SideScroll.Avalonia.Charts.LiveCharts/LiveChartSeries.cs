using Avalonia.Media;
using Avalonia.Threading;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Painting;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Time;
using SideScroll.Utilities;
using SkiaSharp;
using System.Collections;
using System.Collections.Specialized;

namespace SideScroll.Avalonia.Charts.LiveCharts;

/*public class SeriesHoverEventArgs(ListSeries series) : EventArgs
{
	public ListSeries Series => series;
}*/

/// <summary>
/// Wraps a <see cref="SideScroll.Collections.ListSeries"/> for use with LiveCharts, converting source data objects into
/// <see cref="LiveChartPoint"/> instances and subscribing to collection changes for live updates.
/// </summary>
public class LiveChartSeries : IDisposable //: ChartSeries<ISeries>
{
	/// <summary>Gets or sets the maximum number of characters shown in a tooltip series title before truncation.</summary>
	public static int MaxTitleLength { get; set; } = 200;
	/// <summary>Gets or sets the maximum data point count at which individual point markers are drawn.</summary>
	public static int MaxPointsToShowMarkers { get; set; } = 8;
	/// <summary>Gets or sets the default marker geometry size in pixels.</summary>
	public static double DefaultGeometrySize { get; set; } = 5;

	/// <summary>
	/// Gets or sets the maximum number of bins a series is divided into
	/// </summary>
	/// <remarks>
	/// The count comes from the data's own range divided by the configured bin size, so points far
	/// apart with a small size ask for far more bins than a chart has pixels to draw them in. The
	/// bin size widens to fit this instead. <see cref="DateTime"/> values are binned in ticks, where
	/// a year is 3.15e14 of them, so an unfitted size reaches these counts on ordinary data
	/// </remarks>
	public static int MaxBins
	{
		get => _maxBins;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(MaxBins));
			_maxBins = value;
		}
	}
	private static int _maxBins = 10_000;

	/// <summary>Gets the parent chart control.</summary>
	public TabLiveChart Chart { get; }
	/// <summary>Gets the source data series.</summary>
	public ListSeries ListSeries { get; }

	/// <summary>Gets or sets the LiveCharts native line series used for rendering.</summary>
	public LiveChartLineSeries LineSeries { get; set; }
	/// <summary>Gets or sets the converted list of chart data points.</summary>
	public List<LiveChartPoint> DataPoints { get; set; } = []; // Must be initialized for GetDataPoints()

	/// <summary>Gets the SkiaSharp color used for painting this series.</summary>
	public SKColor SkColor { get; protected set; }

	private INotifyCollectionChanged? _boundCollection;

	/// <summary>Returns the underlying <see cref="ListSeries"/>'s string representation.</summary>
	public override string ToString() => ListSeries.ToString();

	/// <summary>Initializes the series, converts the source data to <see cref="LiveChartPoint"/> instances, and subscribes to collection changes.</summary>
	public LiveChartSeries(TabLiveChart chart, ListSeries listSeries, Color color)
	{
		Chart = chart;
		ListSeries = listSeries;

		SkColor = color.AsSkColor();

		// Can't add gaps with ItemSource so convert to LiveChartPoint ourselves
		DataPoints = GetDataPoints(listSeries, listSeries.List);

		LineSeries = new LiveChartLineSeries(this)
		{
			Name = listSeries.Name,
			Values = DataPoints,
			LineSmoothness = 0, // 1 = Curved
			GeometrySize = listSeries.MarkerSize ?? DefaultGeometrySize,
			EnableNullSplitting = true,

			Stroke = new SolidColorPaint(SkColor, (float)listSeries.StrokeThickness),
			GeometryStroke = null,
			GeometryFill = null,
			Fill = null,
		};

		UpdateMarkers();

		if (listSeries.List is INotifyCollectionChanged notifyCollectionChanged)
		{
			_boundCollection = notifyCollectionChanged;
			notifyCollectionChanged.CollectionChanged += List_CollectionChanged;
		}
	}

	/// <summary>
	/// Releases the subscription to the source list's collection changes
	/// </summary>
	/// <remarks>
	/// The source list outlives this series, so the event would otherwise keep the series and the
	/// chart it references alive, and dead charts would keep refreshing on every update.
	/// TabLiveChart disposes its series whenever it clears them
	/// </remarks>
	public void Dispose()
	{
		if (_boundCollection != null)
		{
			_boundCollection.CollectionChanged -= List_CollectionChanged;
			_boundCollection = null;
		}

		GC.SuppressFinalize(this);
	}

	private void List_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		SeriesChanged(ListSeries, e);
	}

	private void UpdateMarkers()
	{
		if (ListSeries.List.Count > 0 && ListSeries.List.Count <= MaxPointsToShowMarkers || HasSinglePoint(DataPoints))
		{
			//LineSeries.GeometryStroke = new SolidColorPaint(skColor, 2f);
			LineSeries.GeometryFill = new SolidColorPaint(SkColor);
		}
		else
		{
			LineSeries.GeometryFill = null;
		}
	}

	private static bool HasSinglePoint(List<LiveChartPoint> dataPoints)
	{
		bool prevNan1 = false;
		bool prevNan2 = false;
		foreach (LiveChartPoint dataPoint in dataPoints)
		{
			bool nan = dataPoint.Y is null or double.NaN;
			if (prevNan2 && !prevNan1 && nan)
				return true;

			prevNan2 = prevNan1;
			prevNan1 = nan;
		}
		return false;
	}

	/// <summary>Returns the series name truncated to <see cref="MaxTitleLength"/> characters for display in the tooltip.</summary>
	public string? GetTooltipTitle()
	{
		string? title = ListSeries.Name;
		if (title != null && title.Length > MaxTitleLength)
		{
			title = title[..MaxTitleLength] + "...";
		}
		return title;
	}

	/// <summary>Builds the tooltip content lines for the given data point, including time, value, tags, and description.</summary>
	public List<string> GetTooltipLines(ChartPoint point)
	{
		List<string> lines = [];

		if (point.Context.DataSource is LiveChartPoint liveChartPoint)
		{
			string valueLabel = ListSeries.YLabel ?? "Value";
			if (liveChartPoint.Object is TimeRangeValue timeRangeValue)
			{
				lines.Add($"Time: {timeRangeValue.TimeText}");
				lines.Add($"Duration: {timeRangeValue.Duration.FormattedDecimal()}");
				lines.Add($"{valueLabel}: {timeRangeValue.Value.Formatted()}");
			}
			else
			{
				if (ListSeries.XPropertyInfo?.PropertyType == typeof(DateTime))
				{
					var startTime = new DateTime((long)liveChartPoint.X!, DateTimeKind.Utc);
					if (ListSeries.PeriodDuration is { } timeSpan)
					{
						string timeText = DateTimeUtils.FormatTimeRange(startTime, startTime.Add(timeSpan), false);
						lines.Add($"Time: {timeText}");
					}
					else
					{
						lines.Add($"Time: {startTime.Format()}");
					}
				}
				else
				{
					lines.Add($"X: {liveChartPoint.X}");
				}
				lines.Add($"{valueLabel}: {liveChartPoint.Y!.Formatted()}");
			}

			if (liveChartPoint.Object is ITags tags && tags.Tags.Count > 0)
			{
				lines.Add("");

				foreach (Tag tag in tags.Tags)
				{
					lines.Add($"{tag.Name}: {tag.Value}");
				}
			}
		}
		if (ListSeries.Description != null)
		{
			lines.Add("");
			lines.AddRange(ListSeries.Description.Split('\n'));
		}
		return lines;
	}

	private List<LiveChartPoint> GetDataPoints(ListSeries listSeries, IList sourceList)
	{
		double x = DataPoints.Count;
		List<LiveChartPoint> chartPoints = [];
		// Faster than using ItemSource?
		foreach (object obj in sourceList)
		{
			if (ListSeries.XPropertyInfo is { } xPropertyInfo)
			{
				object? xObj = xPropertyInfo.GetValue(obj);
				if (xObj is DateTime dateTime)
				{
					x = dateTime.ToUniversalTime().Ticks;
				}
				else if (xObj == null)
				{
					continue;
				}
				else
				{
					x = Convert.ToDouble(xObj);
				}
			}

			double? y = null;
			if (ListSeries.YPropertyInfo is { } yPropertyInfo)
			{
				object? value = yPropertyInfo.GetValue(obj);
				if (value != null)
				{
					y = Convert.ToDouble(value);
				}
			}
			else
			{
				y = Convert.ToDouble(obj);
			}

			if (y != null && double.IsNaN(y.Value))
			{
				y = null;
			}

			double? yCoordinate = null;
			if (y != null && Chart.ChartView.LogBase is { } logBase)
			{
				if (y.Value == 0)
				{
					yCoordinate = 0;
				}
				else
				{
					yCoordinate = Math.Log(y.Value, logBase);
				}
			}

			var chartPoint = new LiveChartPoint(obj, x++, y, yCoordinate);
			chartPoints.Add(chartPoint);
		}

		chartPoints = chartPoints
			.OrderBy(d => d.X)
			.ToList();

		if (chartPoints.Count > 0 && listSeries.XBinSize > 0)
		{
			chartPoints = BinDataPoints(chartPoints, listSeries.XBinSize);
		}
		return chartPoints;
	}

	internal static List<LiveChartPoint> BinDataPoints(List<LiveChartPoint> dataPoints, double xBinSize)
	{
		if (dataPoints.Count == 0) return dataPoints;

		double firstX = dataPoints.First().X!.Value;
		double firstBinX = Math.Floor(firstX / xBinSize) * xBinSize; // use start of interval
		double lastBinX = dataPoints.Last().X!.Value;

		// A NaN or infinite X divides into no meaningful number of bins, and binning it anyway
		// produced a count the conversion below turned into an allocation size
		double range = lastBinX - firstBinX;
		if (!double.IsFinite(range)) return dataPoints;

		xBinSize = FitBinSize(range, xBinSize);

		// The last point's bin is the highest one, rounding up would add an empty bin after it
		int numBins = (int)Math.Floor(range / xBinSize) + 1;

		var bins = new double[numBins];
		var counts = new int[numBins]; // Tracked separately so an empty bin can be told apart from one summing to zero
		foreach (LiveChartPoint dataPoint in dataPoints)
		{
			// A null Y is the gap GetDataPoints() creates for a NaN, so it contributes nothing.
			// Dereferencing it threw, and leaving the count alone lets a bin holding only gaps stay
			// empty, which the loop below already turns back into one
			if (dataPoint.Y is not { } y) continue;

			int bin = (int)Math.Floor((dataPoint.X!.Value - firstBinX) / xBinSize);
			bins[bin] += y;
			counts[bin]++;
		}

		bool prevNan = false;
		List<LiveChartPoint> binDataPoints = [];
		for (int i = 0; i < numBins; i++)
		{
			double value = bins[i];
			if (counts[i] == 0)
			{
				// Only add one NaN per gap, it just tells the chart to break the line
				if (prevNan) continue;

				prevNan = true;
				value = double.NaN;
			}
			else
			{
				prevNan = false;
			}
			binDataPoints.Add(new LiveChartPoint(null, firstBinX + i * xBinSize, value, null));
		}

		return binDataPoints;
	}

	/// <summary>
	/// Widens <paramref name="xBinSize"/> until <paramref name="range"/> divides into no more than
	/// <see cref="MaxBins"/> bins, leaving a size that already fits unchanged
	/// </summary>
	/// <remarks>
	/// Two arrays are allocated per bin, so an unfitted count spent 12 bytes on each of them: a
	/// billion bins claimed around 12 GB while still appearing to succeed, since the pages are only
	/// committed as they're written. Beyond that the count stopped being representable, and the
	/// conversion to <see cref="int"/> saturates rather than wrapping, so the increment after it
	/// turned the size negative and threw <see cref="OverflowException"/> instead
	/// </remarks>
	internal static double FitBinSize(double range, double xBinSize)
	{
		if (range <= 0) return xBinSize;

		double numBins = Math.Floor(range / xBinSize) + 1;
		if (numBins <= MaxBins) return xBinSize;

		// Measured against the same origin, so the bins the caller goes on to fill still cover it
		return range / MaxBins;
	}

	private void SeriesChanged(ListSeries listSeries, NotifyCollectionChangedEventArgs e)
	{
		lock (Chart.Chart.SyncContext)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				var dataPoints = GetDataPoints(listSeries, e.NewItems!);
				DataPoints.AddRange(dataPoints);
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				var dataPoints = GetDataPoints(listSeries, e.OldItems!);
				foreach (LiveChartPoint datapoint in dataPoints)
				{
					DataPoints.RemoveAll(point => point.X == datapoint.X);
				}
			}

			UpdateMarkers();
		}

		Dispatcher.UIThread.InvokeAsync(Chart.Refresh, DispatcherPriority.Background);
	}
}
