using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.Core
{
	public enum SeriesType
	{
		Average,
		Sum,
		Count,
		Minimum,
		Maximum,
		Other,
	}

	public class ListSeries
	{
		public string Name { get; set; }
		public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
		public IList List; // List to start with, any elements added will also trigger an event to add new points

		public PropertyInfo XPropertyInfo; // optional
		public PropertyInfo YPropertyInfo; // optional

		public string XPropertyName;
		public string YPropertyName;
		public string YPropertyLabel;
		public double XBinSize;
		public string Description { get; set; }

		public bool IsStacked { get; set; }
		public TimeSpan? PeriodDuration { get; set; }
		public SeriesType SeriesType { get; set; } = SeriesType.Sum;
		public double Total { get; set; }

		public override string ToString() => Name + "[" + List?.Count + "]";

		public ListSeries(IList list)
		{
			LoadList(list);
		}

		public ListSeries(string name, IList list)
		{
			Name = name;
			LoadList(list);
		}

		public ListSeries(IList list, PropertyInfo xPropertyInfo, PropertyInfo yPropertyInfo)
		{
			List = list;
			XPropertyInfo = xPropertyInfo;
			YPropertyInfo = yPropertyInfo;

			Name = yPropertyInfo.Name.WordSpaced();
			NameAttribute attribute = yPropertyInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
		}

		public ListSeries(string name, IList list, string xPropertyName, string yPropertyName = null)
		{
			Name = name;
			List = list;
			XPropertyName = xPropertyName;
			YPropertyName = yPropertyName;

			Type elementType = list.GetType().GetElementTypeForAll();
			XPropertyInfo = elementType.GetProperty(xPropertyName);
			if (yPropertyName != null)
				YPropertyInfo = elementType.GetProperty(yPropertyName);
		}

		private void LoadList(IList list)
		{
			List = list;

			if (list == null)
				return;

			Type elementType = list.GetType().GetElementTypeForAll();
			XPropertyInfo = elementType.GetPropertyWithAttribute<XAxisAttribute>();
			YPropertyInfo = elementType.GetPropertyWithAttribute<YAxisAttribute>();
		}

		private double GetObjectValue(object obj)
		{
			double value = Convert.ToDouble(obj);
			if (double.IsNaN(value))
				return 0;

			return value;
		}

		public double CalculateTotal(TimeWindow timeWindow = null)
		{
			timeWindow = timeWindow?.Selection ?? timeWindow;
			Total = GetTotal(timeWindow);
			if (Total > 50)
				Total = Math.Floor(Total);
			return Total;
		}

		public double GetTotal(TimeWindow timeWindow)
		{
			var timeRangeValues = TimeRangeValues;
			if (timeWindow == null || PeriodDuration == null || timeRangeValues == null)
				return GetSum();

			return SeriesType switch
			{
				SeriesType.Count => TimeRangePeriod.TotalCounts(timeRangeValues, timeWindow, PeriodDuration.Value),
				SeriesType.Sum => TimeRangePeriod.TotalSum(timeRangeValues, timeWindow, PeriodDuration.Value),
				SeriesType.Minimum => TimeRangePeriod.TotalMinimum(timeRangeValues, timeWindow),
				SeriesType.Maximum => TimeRangePeriod.TotalMaximum(timeRangeValues, timeWindow),
				_ => TimeRangePeriod.TotalAverage(timeRangeValues, timeWindow, PeriodDuration.Value),
			};
		}

		public List<TimeRangeValue> GroupByPeriod(TimeWindow timeWindow)
		{
			var timeRangeValues = TimeRangeValues;
			if (timeWindow == null || PeriodDuration == null || timeRangeValues == null)
				return timeRangeValues;

			return SeriesType switch
			{
				SeriesType.Sum => TimeRangePeriod.PeriodSums(timeRangeValues, timeWindow, PeriodDuration.Value),
				SeriesType.Minimum => TimeRangePeriod.PeriodMins(timeRangeValues, timeWindow, PeriodDuration.Value),
				SeriesType.Maximum => TimeRangePeriod.PeriodMaxes(timeRangeValues, timeWindow, PeriodDuration.Value),
				_ => TimeRangePeriod.PeriodAverages(timeRangeValues, timeWindow, PeriodDuration.Value),
			};
		}

		public double GetSum()
		{
			double sum = 0;
			if (YPropertyInfo != null)
			{
				foreach (object obj in List)
				{
					object value = YPropertyInfo.GetValue(obj);
					if (value != null)
						sum += GetObjectValue(value);
				}
			}
			else
			{
				foreach (object obj in List)
				{
					double value = GetObjectValue(obj);
					sum += value;
				}
			}
			return sum;
		}

		public List<TimeRangeValue> TimeRangeValues
		{
			get
			{
				if (XPropertyInfo?.PropertyType != typeof(DateTime))
					return null;

				var timeRangeValues = new List<TimeRangeValue>();
				foreach (object obj in List)
				{
					if (obj is TimeRangeValue timeRangeValue)
					{
						timeRangeValues.Add(timeRangeValue);
						continue;
					}

					DateTime timeStamp = (DateTime)XPropertyInfo.GetValue(obj);
					double value = 1;
					if (YPropertyInfo != null)
					{
						object yObj = YPropertyInfo.GetValue(obj);
						value = Convert.ToDouble(yObj);
					}
					timeRangeValue = new TimeRangeValue(timeStamp, timeStamp, value);
					timeRangeValues.Add(timeRangeValue);
				}
				var ordered = timeRangeValues.OrderBy(t => t.StartTime).ToList();
				return ordered;
			}
		}

		public TimeWindow GetTimeWindow()
		{
			DateTime startTime = DateTime.MaxValue;
			DateTime endTime = DateTime.MinValue;
			foreach (TimeRangeValue timeRangeValue in TimeRangeValues)
			{
				startTime = startTime.Min(timeRangeValue.StartTime);
				endTime = endTime.Max(timeRangeValue.EndTime);
			}
			return new TimeWindow(startTime, endTime);
		}
	}
}
