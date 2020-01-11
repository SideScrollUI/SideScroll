using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Atlas.Extensions;

namespace Atlas.Core
{
	public class ListGroup
	{
		public string Name { get; set; }
		public bool Horizontal { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public ItemCollection<ListSeries> ListSeries { get; set; } = new ItemCollection<ListSeries>();

		public double xBinSize;

		public ListGroup(string name = null, DateTime? startTime = null, DateTime? endTime = null)
		{
			Name = name;
			StartTime = startTime;
			EndTime = endTime;
		}

		public override string ToString()
		{
			return Name;
		}

		public void AddDimensions(IList iList, string categoryPropertyName, string xPropertyName, string yPropertyName)
		{
			Type listType = iList.GetType();
			Type elementType = iList.GetType().GetElementTypeForAll();
			PropertyInfo categoryPropertyInfo = elementType.GetProperty(categoryPropertyName);

			var dimensions = new Dictionary<string, IList>();
			foreach (var obj in iList)
			{
				var categoryObject = categoryPropertyInfo.GetValue(obj);

				string category = categoryObject.ToString();

				IList categoryList;
				if (!dimensions.TryGetValue(category, out categoryList))
				{
					categoryList = (IList)Activator.CreateInstance(listType);
					dimensions.Add(category, categoryList);

					ListSeries listSeries = new ListSeries(category, categoryList, xPropertyName, yPropertyName)
					{
						xBinSize = xBinSize,
					};
					ListSeries.Add(listSeries);
				}
				categoryList.Add(obj);
			}
			SortBySum();
		}

		public void SortBySum()
		{
			var sums = new Dictionary<ListSeries, double>();
			foreach (var listSeries in ListSeries)
				sums.Add(listSeries, listSeries.GetSum());

			var sortedDict = from entry in sums orderby entry.Value descending select entry.Key;

			ListSeries = new ItemCollection<ListSeries>(sortedDict);
		}
	}

	public class ListSeries
	{
		public string Name { get; set; }
		public IList iList; // List to start with, any elements added will also trigger an event to add new points
		//public PropertyInfo xPropertyInfo; // optional

		public PropertyInfo xPropertyInfo; // optional
		public PropertyInfo yPropertyInfo; // optional

		public string xPropertyName;
		public string yPropertyName;
		//public object obj;
		public double xBinSize;

		public bool IsStacked { get; set; }

		public ListSeries(string name, IList iList)
		{
			Name = name;
			this.iList = iList;

			Type elementType = iList.GetType().GetElementTypeForAll();
			xPropertyInfo = elementType.GetPropertyWithAttribute<XAxisAttribute>();
			yPropertyInfo = elementType.GetPropertyWithAttribute<YAxisAttribute>();
		}

		public ListSeries(IList iList, PropertyInfo xPropertyInfo, PropertyInfo yPropertyInfo)
		{
			this.iList = iList;
			this.xPropertyInfo = xPropertyInfo;
			this.yPropertyInfo = yPropertyInfo;

			Name = yPropertyInfo.Name.AddSpacesBetweenWords();
			NameAttribute attribute = yPropertyInfo.GetCustomAttribute<NameAttribute>();
			if (attribute != null)
				Name = attribute.Name;
		}

		public ListSeries(string name, IList iList, string xPropertyName, string yPropertyName)
		{
			Name = name;
			this.iList = iList;
			this.xPropertyName = xPropertyName;
			this.yPropertyName = yPropertyName;

			Type elementType = iList.GetType().GetElementTypeForAll();
			xPropertyInfo = elementType.GetProperty(xPropertyName);
			yPropertyInfo = elementType.GetProperty(yPropertyName);
		}

		public override string ToString()
		{
			return Name;
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
