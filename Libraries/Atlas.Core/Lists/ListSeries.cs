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
		public double XBinSize;
		public string Description { get; set; }

		public bool IsStacked { get; set; }
		public TimeSpan? PeriodDuration { get; set; }
		public SeriesType SeriesType { get; set; } = SeriesType.Sum;
		public double Total { get; set; }

		public override string ToString() => Name;

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

		public double CalculateTotal(TimeWindow timeWindow)
		{
			Total = GetTotal(timeWindow);
			if (Total > 50)
				Total = Math.Floor(Total);
			return Total;
		}

		public double GetTotal(TimeWindow timeWindow)
		{
			if (timeWindow == null || PeriodDuration == null || XPropertyInfo == null || XPropertyInfo.PropertyType != typeof(DateTime))
				return GetSum();

			switch (SeriesType)
			{
				case SeriesType.Sum: return TimeRangePeriod.TotalSum(TimeRangeValues, timeWindow, PeriodDuration.Value);
				case SeriesType.Minimum: return TimeRangePeriod.TotalMinimum(TimeRangeValues);
				case SeriesType.Maximum: return TimeRangePeriod.TotalMaximum(TimeRangeValues);
				default: return TimeRangePeriod.TotalAverage(TimeRangeValues, timeWindow, PeriodDuration.Value);
			}
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
				var timeRangeValues = new List<TimeRangeValue>();
				foreach (object obj in List)
				{
					DateTime timeStamp = (DateTime)XPropertyInfo.GetValue(obj);
					double value = 1;
					if (YPropertyInfo != null)
					{
						object yObj = YPropertyInfo.GetValue(obj);
						value = Convert.ToDouble(yObj);
					}
					var timeRangeValue = new TimeRangeValue(timeStamp, timeStamp, value);
					timeRangeValues.Add(timeRangeValue);
				}
				var ordered = timeRangeValues.OrderBy(t => t.StartTime).ToList();
				return ordered;
			}
		}
	}
}
