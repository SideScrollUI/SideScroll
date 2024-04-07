using Atlas.Core;
using Atlas.Core.Utilities;
using Atlas.Extensions;
using Avalonia.Media;
using Avalonia.Threading;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Atlas.UI.Avalonia.Charts.LiveCharts;

public class SeriesHoverEventArgs(ListSeries series) : EventArgs
{
	public ListSeries Series { get; set; } = series;
}

public class LiveChartSeries //: ChartSeries<ISeries>
{
	private const int MaxPointsToShowMarkers = 8;
	private const int MaxTitleLength = 200;
	private const double DefaultGeometrySize = 5;

	public readonly TabControlLiveChart Chart;
	public readonly ListSeries ListSeries;
	public readonly bool UseDateTimeAxis;

	public LiveChartLineSeries LineSeries;
	public List<LiveChartPoint> DataPoints = new();

	public override string? ToString() => ListSeries?.ToString();

	public LiveChartSeries(TabControlLiveChart chart, ListSeries listSeries, Color color, bool useDateTimeAxis)
	{
		Chart = chart;
		ListSeries = listSeries;
		UseDateTimeAxis = useDateTimeAxis;

		SKColor skColor = color.AsSkColor();

		// Can't add gaps with ItemSource so convert to LiveChartPoint ourselves
		DataPoints = GetDataPoints(listSeries, listSeries.List);

		LineSeries = new LiveChartLineSeries(this)
		{
			Name = listSeries.Name,
			Values = DataPoints,
			LineSmoothness = 0, // 1 = Curved
			GeometrySize = listSeries.MarkerSize ?? DefaultGeometrySize,
			EnableNullSplitting = true,

			Stroke = new SolidColorPaint(skColor, (float)listSeries.StrokeThickness),
			GeometryStroke = null,
			GeometryFill = null,
			Fill = null,
		};

		if (listSeries.List.Count > 0 && listSeries.List.Count <= MaxPointsToShowMarkers || HasSinglePoint(DataPoints))
		{
			//LineSeries.GeometryStroke = new SolidColorPaint(skColor, 2f);
			LineSeries.GeometryFill = new SolidColorPaint(skColor);
		}

		if (listSeries.List is INotifyCollectionChanged iNotifyCollectionChanged)
		{
			//iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;
			iNotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(delegate (object? sender, NotifyCollectionChangedEventArgs e)
			{
				// Can we remove this later when disposing?
				SeriesChanged(listSeries, e);
			});
		}
	}

	private bool HasSinglePoint(List<LiveChartPoint> dataPoints)
	{
		bool prevNan1 = false;
		bool prevNan2 = false;
		foreach (LiveChartPoint dataPoint in dataPoints)
		{
			bool nan = dataPoint.Y == null || double.IsNaN(dataPoint.Y.Value);
			if (prevNan2 && !prevNan1 && nan)
				return true;

			prevNan2 = prevNan1;
			prevNan1 = nan;
		}
		return false;
	}

	public string? GetTooltipTitle()
	{
		string? title = ListSeries.Name;
		if (title != null && title.Length > MaxTitleLength)
		{
			title = title[..MaxTitleLength] + "...";
		}
		return title;
	}

	public string[] GetTooltipLines(ChartPoint point)
	{
		List<string> lines = new();

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
					var startTime = new DateTime((long)liveChartPoint.X!);
					if (ListSeries.PeriodDuration is TimeSpan timeSpan)
					{
						string timeText = DateTimeUtils.FormatTimeRange(startTime, startTime.Add(timeSpan), false);
						lines.Add($"Time: {timeText}");
					}
					else
					{
						lines.Add($"Time: {startTime.Formatted()}");
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
		return lines.ToArray();
	}

	private List<LiveChartPoint> GetDataPoints(ListSeries listSeries, IList iList)
	{
		double x = DataPoints.Count;
		var chartPoints = new List<LiveChartPoint>();
		// Faster than using ItemSource?
		foreach (object obj in iList)
		{
			if (ListSeries.XPropertyInfo is PropertyInfo xPropertyInfo)
			{
				object? xObj = xPropertyInfo.GetValue(obj);
				if (xObj is DateTime dateTime)
				{
					x = dateTime.Ticks;
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
			if (ListSeries.YPropertyInfo is PropertyInfo yPropertyInfo)
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
			if (y != null && Chart.ChartView.LogBase is double logBase)
			{
				yCoordinate = Math.Log(y.Value, logBase);
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

	private static List<LiveChartPoint> BinDataPoints(List<LiveChartPoint> dataPoints, double xBinSize)
	{
		if (dataPoints.Count == 0) return dataPoints;

		double firstX = dataPoints.First().X!.Value;
		double firstBinX = ((int)(firstX / xBinSize)) * xBinSize; // use start of interval
		double lastBinX = dataPoints.Last()!.X!.Value;
		int numBins = (int)Math.Ceiling((lastBinX - firstBinX) / xBinSize) + 1;
		double[] bins = new double[numBins];
		foreach (LiveChartPoint dataPoint in dataPoints)
		{
			int bin = (int)((dataPoint.X!.Value - firstBinX) / xBinSize);
			bins[bin] += dataPoint.Y!.Value;
		}

		bool prevNan = false;
		var binDataPoints = new List<LiveChartPoint>();
		for (int i = 0; i < numBins; i++)
		{
			double value = bins[i];
			if (value == 0)
			{
				if (prevNan) continue;

				prevNan = true;
				value = double.NaN;
			}
			else
			{
				prevNan = true;
			}
			binDataPoints.Add(new LiveChartPoint(null, firstBinX + i * xBinSize, value, null));
		}

		return binDataPoints;
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
		}

		Dispatcher.UIThread.InvokeAsync(Chart.Refresh, DispatcherPriority.Background);
	}
}
