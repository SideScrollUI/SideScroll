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

namespace Atlas.UI.Avalonia.Controls
{
	public class TabChartLineSeries : OxyPlot.Series.LineSeries
	{
		public TabControlChart chart;
		public ListSeries listSeries;
		public PropertyInfo xAxisPropertyInfo;

		// DataPoint is sealed
		public Dictionary<DataPoint, object> datapointLookup = new Dictionary<DataPoint, object>();

		private bool useDateTimeAxis;

		public TabChartLineSeries(TabControlChart chart, ListSeries listSeries, bool useDateTimeAxis)
		{
			this.chart = chart;
			this.listSeries = listSeries;
			this.useDateTimeAxis = useDateTimeAxis;

			InitializeComponent(listSeries);
		}

		private void InitializeComponent(ListSeries listSeries)
		{
			Title = listSeries.Name;
			if (Title?.Length == 0)
				Title = "<NA>";
			LineStyle = LineStyle.Solid;
			StrokeThickness = 2;
			TextColor = OxyColors.Black;
			CanTrackerInterpolatePoints = false;
			MinimumSegmentLength = 2;
			MarkerSize = 3;
			MarkerType = listSeries.iList.Count < 20 ? MarkerType.Circle : MarkerType.None;
			LoadTrackFormat();

			// can't add gaps with ItemSource so convert to DataPoint ourselves
			var dataPoints = GetDataPoints(listSeries, listSeries.iList, datapointLookup);
			Points.AddRange(dataPoints);

			if (listSeries.iList is INotifyCollectionChanged iNotifyCollectionChanged)
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

		public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
		{
			TrackerHitResult result = base.GetNearestPoint(point, interpolate);
			if (result == null)
				return null;

			if (datapointLookup.TryGetValue(result.DataPoint, out object obj))
			{
				if (obj is ITags tags && tags.Tags.Count > 0)
				{
					result.Text += "\n";

					foreach (Tag tag in tags.Tags)
					{
						result.Text += "\n" + tag.Name + ": " + tag.Value;
					}
				}
				/*if (DescriptionProperty is PropertyInfo propertyInfo)
				{
					object value = propertyInfo.GetValue(obj);
					if (value is string text && text.Length > 0)
						result.Text += "\n\n" + text;
				}*/
			}
			if (listSeries.Description != null)
				result.Text += "\n\n" + listSeries.Description;
			return result;
		}

		/*private PropertyInfo DescriptionProperty
		{
			get
			{
				Type type = listSeries.iList[0].GetType();
				var props = type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DescriptionAttribute)));
				if (props.ToList().Count == 0)
					return null;
				return props.First() as PropertyInfo;
			}
		}*/

		private void LoadTrackFormat()
		{
			string xTrackerFormat = listSeries.xPropertyName ?? "Index: {2:#,0.###}";
			if (useDateTimeAxis || listSeries.xPropertyInfo?.PropertyType == typeof(DateTime))
				xTrackerFormat = "Time: {2:yyyy-M-d H:mm:ss.FFF}";
			TrackerFormatString = "{0}\n" + xTrackerFormat + "\nValue: {4:#,0.###}";
			/*if (listSeries.iList.Count > 0)
			{
				if (DescriptionProperty != null)
					TrackerFormatString += "\n\nDescription {" + DescriptionProperty.Name + "}";
			}*/
		}

		private List<DataPoint> GetDataPoints(ListSeries listSeries, IList iList, Dictionary<DataPoint, object> datapointLookup = null)
		{
			UpdateXAxisProperty(listSeries);
			double x = Points.Count;
			var dataPoints = new List<DataPoint>();
			if (listSeries.yPropertyInfo != null)
			{
				// faster than using ItemSource?
				foreach (object obj in iList)
				{
					object value = listSeries.yPropertyInfo.GetValue(obj);
					if (xAxisPropertyInfo != null)
					{
						object xObj = xAxisPropertyInfo.GetValue(obj);
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

				if (dataPoints.Count > 0 && listSeries.xBinSize > 0)
				{
					dataPoints = BinDataPoints(listSeries, dataPoints);
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
			if (listSeries.yPropertyInfo != null)
			{
				if (listSeries.xPropertyInfo != null)
					xAxisPropertyInfo = listSeries.xPropertyInfo;
				if (xAxisPropertyInfo == null)
				{
					Type elementType = listSeries.iList.GetType().GetElementTypeForAll();
					foreach (PropertyInfo propertyInfo in elementType.GetProperties())
					{
						if (propertyInfo.GetCustomAttribute<XAxisAttribute>() != null)
							xAxisPropertyInfo = propertyInfo;
					}
				}
			}
		}

		private static List<DataPoint> BinDataPoints(ListSeries listSeries, List<DataPoint> dataPoints)
		{
			double firstBin = dataPoints.First().X;
			double lastBin = dataPoints.Last().X;
			int numBins = (int)Math.Ceiling((lastBin - firstBin) / listSeries.xBinSize) + 1;
			double[] bins = new double[numBins];
			foreach (DataPoint dataPoint in dataPoints)
			{
				int bin = (int)((dataPoint.X - firstBin) / listSeries.xBinSize);
				bins[bin] += dataPoint.Y;
			}
			var binDataPoints = new List<DataPoint>();
			for (int i = 0; i < numBins; i++)
			{
				binDataPoints.Add(new DataPoint(firstBin + i * listSeries.xBinSize, bins[i]));
			}

			return binDataPoints;
		}

		private void SeriesChanged(ListSeries listSeries, IList iList)
		{
			lock (chart.PlotModel.SyncRoot)
			{
				//this.Update();
				var dataPoints = GetDataPoints(listSeries, iList);
				Points.AddRange(dataPoints);
			}

			Dispatcher.UIThread.InvokeAsync(() => chart.Refresh(), DispatcherPriority.Background);
		}
	}
}
