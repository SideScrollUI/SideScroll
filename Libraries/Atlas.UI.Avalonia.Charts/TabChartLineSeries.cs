using Atlas.Core;
using Atlas.Extensions;
using Avalonia.Threading;
using OxyPlot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace Atlas.UI.Avalonia.Charts;

public class TabChartLineSeries : OxyPlot.Series.LineSeries
{
	private const int MaxPointsToShowMarkers = 8;
	private const int MaxTitleLength = 200;

	public readonly TabControlChart Chart;
	public readonly ListSeries ListSeries;
	public readonly bool UseDateTimeAxis;

	public PropertyInfo XAxisPropertyInfo;

	// DataPoint is sealed
	private readonly Dictionary<DataPoint, object> _datapointLookup = new();

	public override string ToString() => ListSeries?.ToString();

	public TabChartLineSeries(TabControlChart chart, ListSeries listSeries, bool useDateTimeAxis)
	{
		Chart = chart;
		ListSeries = listSeries;
		UseDateTimeAxis = useDateTimeAxis;

		InitializeComponent(listSeries);
	}

	private void InitializeComponent(ListSeries listSeries)
	{
		// Title must be unique among all series
		Title = listSeries.Name;
		if (Title?.Length == 0)
			Title = "<NA>";

		LineStyle = LineStyle.Solid;
		StrokeThickness = 2;
		TextColor = OxyColors.Black;
		CanTrackerInterpolatePoints = false;
		MinimumSegmentLength = 2;
		MarkerSize = 3;
		MarkerType = listSeries.List.Count <= MaxPointsToShowMarkers ? MarkerType.Circle : MarkerType.None;
		LoadTrackFormat();

		// can't add gaps with ItemSource so convert to DataPoint ourselves
		var dataPoints = GetDataPoints(listSeries, listSeries.List, _datapointLookup);
		Points.AddRange(dataPoints);

		if (listSeries.List is INotifyCollectionChanged iNotifyCollectionChanged)
		{
			//iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;
			iNotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(delegate (object sender, NotifyCollectionChangedEventArgs e)
			{
				// can we remove this later when disposing?
				SeriesChanged(listSeries, e.NewItems);
			});
		}

		// use circle markers if there's a single point all alone, otherwise it won't show
		if (HasSinglePoint())
			MarkerType = MarkerType.Circle;
	}

	private bool HasSinglePoint()
	{
		bool prevNan1 = false;
		bool prevNan2 = false;
		foreach (DataPoint dataPoint in Points)
		{
			bool nan = double.IsNaN(dataPoint.Y);
			if (prevNan2 && !prevNan1 && nan)
				return true;

			prevNan2 = prevNan1;
			prevNan1 = nan;
		}
		return false;
	}

	// Override the default tracker text
	public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
	{
		TrackerHitResult result = base.GetNearestPoint(point, interpolate);
		if (result == null)
			return null;

		string valueLabel = ListSeries.YPropertyLabel ?? "Value";

		if (_datapointLookup.TryGetValue(result.DataPoint, out object obj))
		{
			if (obj is TimeRangeValue timeRangeValue)
			{
				string title = ListSeries.Name;
				if (title != null && title.Length > MaxTitleLength)
					title = title[..MaxTitleLength] + "...";

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
		string xTrackerFormat = ListSeries.XPropertyName ?? "Index: {2:#,0.###}";
		if (UseDateTimeAxis || ListSeries.XPropertyInfo?.PropertyType == typeof(DateTime))
		{
			xTrackerFormat = "Time: {2:yyyy-M-d H:mm:ss.FFF}";
		}
		TrackerFormatString = "{0}\n" + xTrackerFormat + "\nValue: {4:#,0.###}";
	}

	private List<DataPoint> GetDataPoints(ListSeries listSeries, IList iList, Dictionary<DataPoint, object> datapointLookup = null)
	{
		UpdateXAxisProperty(listSeries);
		double x = Points.Count;
		var dataPoints = new List<DataPoint>();
		if (listSeries.YPropertyInfo != null)
		{
			// faster than using ItemSource?
			foreach (object obj in iList)
			{
				object value = listSeries.YPropertyInfo.GetValue(obj);
				if (XAxisPropertyInfo != null)
				{
					object xObj = XAxisPropertyInfo.GetValue(obj);
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
				double d = double.NaN;
				if (value != null)
					d = Convert.ToDouble(value);

				var dataPoint = new DataPoint(x++, d);
				if (datapointLookup != null && !double.IsNaN(d) && !datapointLookup.ContainsKey(dataPoint))
					datapointLookup.Add(dataPoint, obj);
				dataPoints.Add(dataPoint);
			}
			dataPoints = dataPoints.OrderBy(d => d.X).ToList();

			if (dataPoints.Count > 0 && listSeries.XBinSize > 0)
			{
				dataPoints = BinDataPoints(listSeries, dataPoints, listSeries.XBinSize);
			}
		}
		else
		{
			foreach (object obj in iList)
			{
				double value = Convert.ToDouble(obj);
				dataPoints.Add(new DataPoint(x++, value));
			}
		}
		return dataPoints;
	}

	private void UpdateXAxisProperty(ListSeries listSeries)
	{
		if (listSeries.YPropertyInfo != null)
		{
			if (listSeries.XPropertyInfo != null)
				XAxisPropertyInfo = listSeries.XPropertyInfo;

			if (XAxisPropertyInfo == null)
			{
				Type elementType = listSeries.List.GetType().GetElementTypeForAll();
				foreach (PropertyInfo propertyInfo in elementType.GetProperties())
				{
					if (propertyInfo.GetCustomAttribute<XAxisAttribute>() != null)
						XAxisPropertyInfo = propertyInfo;
				}
			}
		}
	}

	private static List<DataPoint> BinDataPoints(ListSeries listSeries, List<DataPoint> dataPoints, double xBinSize)
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
				if (prevNan)
					continue;

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

	private void SeriesChanged(ListSeries listSeries, IList iList)
	{
		lock (Chart.PlotModel.SyncRoot)
		{
			//this.Update();
			var dataPoints = GetDataPoints(listSeries, iList);
			Points.AddRange(dataPoints);
		}

		Dispatcher.UIThread.InvokeAsync(() => Chart.Refresh(), DispatcherPriority.Background);
	}
}
