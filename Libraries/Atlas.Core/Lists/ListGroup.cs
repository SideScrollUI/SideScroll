using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.Core
{
	public class ListGroup
	{
		public string Name { get; set; }
		public bool Horizontal { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public bool ShowOrder { get; set; } = true;
		public string UnitName { get; set; }
		public bool ShowLegend { get; set; } = true;
		public bool ShowTimeTracker { get; set; } = false;
		public double? MinValue { get; set; }
		public ItemCollection<ListSeries> Series { get; set; } = new ItemCollection<ListSeries>();

		public double XBinSize { get; set; }

		public override string ToString() => Name;

		public ListGroup(string name = null, DateTime? startTime = null, DateTime? endTime = null)
		{
			Name = name;
			StartTime = startTime;
			EndTime = endTime;
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

				if (!dimensions.TryGetValue(category, out IList categoryList))
				{
					categoryList = (IList)Activator.CreateInstance(listType);
					dimensions.Add(category, categoryList);

					var listSeries = new ListSeries(category, categoryList, xPropertyName, yPropertyName)
					{
						XBinSize = XBinSize,
					};
					Series.Add(listSeries);
				}
				categoryList.Add(obj);
			}
			SortBySum();
		}

		public void SortBySum()
		{
			var sums = new Dictionary<ListSeries, double>();
			foreach (var listSeries in Series)
				sums.Add(listSeries, listSeries.GetSum());

			var sortedDict = from entry in sums orderby entry.Value descending select entry.Key;

			Series = new ItemCollection<ListSeries>(sortedDict);
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
