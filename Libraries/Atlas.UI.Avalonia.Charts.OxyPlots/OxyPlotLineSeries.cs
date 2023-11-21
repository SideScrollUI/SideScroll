using Atlas.Core;
using Atlas.Extensions;
using Avalonia.Media;
using Avalonia.Threading;
using OxyPlot;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Atlas.UI.Avalonia.Charts.OxyPlots;

public class OxyPlotChartSeries : ChartSeries<OxyPlotLineSeries>
{
	public OxyPlotChartSeries(ListSeries listSeries, OxyPlotLineSeries lineSeries, Color color) :
		base(listSeries, lineSeries, color)
	{ }
}

public class OxyPlotLineSeries : OxyPlot.Series.LineSeries
{
	private const int MaxPointsToShowMarkers = 8;
	private const int MaxTitleLength = 200;

	public readonly TabControlOxyPlot Chart;
	public readonly ListSeries ListSeries;
	public readonly bool UseDateTimeAxis;

	// DataPoint is a struct which can't be inherited so we need a lookup
	private readonly Dictionary<DataPoint, object> _datapointLookup = new();

	public PropertyInfo? XPropertyInfo => ListSeries.XPropertyInfo;

	public override string? ToString() => ListSeries?.Name;

	public OxyPlotLineSeries(TabControlOxyPlot chart, ListSeries listSeries, bool useDateTimeAxis)
	{
		Chart = chart;
		ListSeries = listSeries;
		UseDateTimeAxis = useDateTimeAxis;

		// Title must be unique among all series
		Title = listSeries.Name;
		if (Title?.Length == 0)
		{
			Title = "<NA>";
		}

		LineStyle = LineStyle.Solid;
		StrokeThickness = 2;
		TextColor = OxyColors.Black;
		CanTrackerInterpolatePoints = false;
		MinimumSegmentLength = 2;
		MarkerSize = 3;
		MarkerType = listSeries.List.Count <= MaxPointsToShowMarkers ? MarkerType.Circle : MarkerType.None;

		LoadTrackFormat();

		// Can't add gaps with ItemSource so convert to DataPoint ourselves
		var dataPoints = GetDataPoints(listSeries, listSeries.List, _datapointLookup);
		Points.AddRange(dataPoints);

		if (listSeries.List is INotifyCollectionChanged iNotifyCollectionChanged)
		{
			//iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;
			iNotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(delegate (object? sender, NotifyCollectionChangedEventArgs e)
			{
				// can we remove this later when disposing?
				SeriesChanged(listSeries, e);
			});
		}

		// Use circle markers if there's a single point all alone, otherwise it won't show
		if (HasSinglePoint())
		{
			MarkerType = MarkerType.Circle;
		}
	}

	private bool HasSinglePoint()
	{
		bool prevNan1 = false;
		bool prevNan2 = false;
		foreach (DataPoint dataPoint in Points)
		{
			bool nan = double.IsNaN(dataPoint.Y);
			if (prevNan2 && !prevNan1 && nan) return true;

			prevNan2 = prevNan1;
			prevNan1 = nan;
		}
		return false;
	}

	// Override the default tracker text
	public override TrackerHitResult? GetNearestPoint(ScreenPoint point, bool interpolate)
	{
		TrackerHitResult result = base.GetNearestPoint(point, interpolate);
		if (result == null)
			return null;

		if (_datapointLookup.TryGetValue(result.DataPoint, out object? obj))
		{
			if (obj is TimeRangeValue timeRangeValue)
			{
				string? title = ListSeries.Name;
				if (title != null && title.Length > MaxTitleLength)
				{
					title = title[..MaxTitleLength] + "...";
				}

				string valueLabel = ListSeries.YLabel ?? "Value";
				result.Text = title + "\n\nTime: " + timeRangeValue.TimeText + "\nDuration: " + timeRangeValue.Duration.FormattedDecimal() + "\n" + valueLabel + ": " + timeRangeValue.Value.Formatted();
			}

			if (obj is ITags tags && tags.Tags.Count > 0)
			{
				result.Text += "\n";

				foreach (Tag tag in tags.Tags)
				{
					result.Text += "\n" + tag.Name + ": " + tag.Value;
				}
			}
		}
		if (ListSeries.Description != null)
			result.Text += "\n\n" + ListSeries.Description;
		return result;
	}

	/*
		{0} the title of the series
		{1} the title of the x-axis
		{2} the x-value
		{3} the title of the y-axis
		{4} the y-value
	*/
	private void LoadTrackFormat()
	{
		string label = ListSeries.XLabel ?? XPropertyInfo?.Name ?? "Index";
		string xTrackerFormat = $"{label}: {2:#,0.###}";
		if (UseDateTimeAxis || XPropertyInfo?.PropertyType == typeof(DateTime))
		{
			xTrackerFormat = "Time: {2:yyyy-M-d H:mm:ss.FFF}";
		}
		TrackerFormatString = "{0}\n" + xTrackerFormat + "\nValue: {4:#,0.###}";
	}

	private List<DataPoint> GetDataPoints(ListSeries listSeries, IList iList, Dictionary<DataPoint, object>? datapointLookup = null)
	{
		double x = Points.Count;
		var dataPoints = new List<DataPoint>();
		if (listSeries.YPropertyInfo != null)
		{
			// Faster than using ItemSource?
			foreach (object obj in iList)
			{
				if (XPropertyInfo != null)
				{
					object? xObj = XPropertyInfo.GetValue(obj);
					if (xObj is DateTime dateTime)
					{
						x = OxyPlot.Axes.DateTimeAxis.ToDouble(dateTime);
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

				object? value = listSeries.YPropertyInfo.GetValue(obj);
				double y = double.NaN;
				if (value != null)
				{
					y = Convert.ToDouble(value);
				}

				var dataPoint = new DataPoint(x++, y);
				if (datapointLookup != null && !double.IsNaN(y) && !datapointLookup.ContainsKey(dataPoint))
				{
					datapointLookup.Add(dataPoint, obj);
				}
				dataPoints.Add(dataPoint);
			}
			dataPoints = dataPoints.OrderBy(d => d.X).ToList();
		}
		else
		{
			foreach (object obj in iList)
			{
				double value = Convert.ToDouble(obj);
				dataPoints.Add(new DataPoint(x++, value));
			}
		}

		if (dataPoints.Count > 0 && listSeries.XBinSize > 0)
		{
			dataPoints = BinDataPoints(dataPoints, listSeries.XBinSize);
		}

		return dataPoints;
	}

	private static List<DataPoint> BinDataPoints(List<DataPoint> dataPoints, double xBinSize)
	{
		double firstX = dataPoints.First().X;
		double firstBinX = ((int)(firstX / xBinSize)) * xBinSize; // use start of interval
		double lastBinX = dataPoints.Last().X;
		int numBins = (int)Math.Ceiling((lastBinX - firstBinX) / xBinSize) + 1;
		double[] bins = new double[numBins];
		foreach (DataPoint dataPoint in dataPoints)
		{
			int bin = (int)((dataPoint.X - firstBinX) / xBinSize);
			bins[bin] += dataPoint.Y;
		}

		bool prevNan = false;
		var binDataPoints = new List<DataPoint>();
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
			binDataPoints.Add(new DataPoint(firstBinX + i * xBinSize, value));
		}

		return binDataPoints;
	}

	private void SeriesChanged(ListSeries listSeries, NotifyCollectionChangedEventArgs e)
	{
		lock (Chart.PlotModel!.SyncRoot)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				var dataPoints = GetDataPoints(listSeries, e.NewItems!);
				Points.AddRange(dataPoints);
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				var dataPoints = GetDataPoints(listSeries, e.OldItems!);
				foreach (DataPoint datapoint in dataPoints)
				{
					Points.RemoveAll(point => point.X == datapoint.X);
				}
			}
		}

		Dispatcher.UIThread.InvokeAsync(Chart.Refresh, DispatcherPriority.Background);
	}
}
