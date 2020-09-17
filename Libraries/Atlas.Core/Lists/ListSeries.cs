using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.Core
{
	public class ListSeries
	{
		public string Name { get; set; }
		public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
		public IList iList; // List to start with, any elements added will also trigger an event to add new points
		//public PropertyInfo xPropertyInfo; // optional

		public PropertyInfo xPropertyInfo; // optional
		public PropertyInfo yPropertyInfo; // optional

		public string xPropertyName;
		public string yPropertyName;
		//public object obj;
		public double xBinSize;
		public string Description { get; set; }

		public bool IsStacked { get; set; }

		public override string ToString() => Name;

		public ListSeries(IList iList)
		{
			LoadList(iList);
		}

		public ListSeries(string name, IList iList)
		{
			Name = name;
			LoadList(iList);
		}

		public ListSeries(IList iList, PropertyInfo xPropertyInfo, PropertyInfo yPropertyInfo)
		{
			this.iList = iList;
			this.xPropertyInfo = xPropertyInfo;
			this.yPropertyInfo = yPropertyInfo;

			Name = yPropertyInfo.Name.WordSpaced();
			NameAttribute attribute = yPropertyInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
		}

		public ListSeries(string name, IList iList, string xPropertyName, string yPropertyName = null)
		{
			Name = name;
			this.iList = iList;
			this.xPropertyName = xPropertyName;
			this.yPropertyName = yPropertyName;

			Type elementType = iList.GetType().GetElementTypeForAll();
			xPropertyInfo = elementType.GetProperty(xPropertyName);
			if (yPropertyName != null)
				yPropertyInfo = elementType.GetProperty(yPropertyName);
		}

		private void LoadList(IList iList)
		{
			this.iList = iList;

			Type elementType = iList.GetType().GetElementTypeForAll();
			xPropertyInfo = elementType.GetPropertyWithAttribute<XAxisAttribute>();
			yPropertyInfo = elementType.GetPropertyWithAttribute<YAxisAttribute>();
		}

		private double GetObjectValue(object obj)
		{
			double value = Convert.ToDouble(obj);
			if (double.IsNaN(value))
				return 0;
			return value;
		}

		public double GetSum()
		{
			double sum = 0;
			if (yPropertyInfo != null)
			{
				foreach (object obj in iList)
				{
					object value = yPropertyInfo.GetValue(obj);
					if (value != null)
						sum += GetObjectValue(value);
				}
			}
			else
			{
				foreach (object obj in iList)
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
				foreach (object obj in iList)
				{
					DateTime timeStamp = (DateTime)xPropertyInfo.GetValue(obj);
					double value = 1;
					if (yPropertyInfo != null)
					{
						object yObj = yPropertyInfo.GetValue(obj);
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

/*
TabSeries
	Pros
		Has iList
			
ListChart(x) -> ListChartItem(x) -> ListSeries -> ItemSeries -> ItemList
	Cons
		Bad name

Binding an iList
Who should update


*/
