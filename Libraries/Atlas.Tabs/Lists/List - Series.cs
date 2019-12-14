using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	public class ListGroup
	{
		public string Name { get; set; }
		public bool Horizontal { get; set; }
		public ItemCollection<ListSeries> ListSeries { get; set; } = new ItemCollection<ListSeries>();
		public ListGroup(string name = null)
		{
			Name = name;
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

		public ListSeries(string name, IList iList)
		{
			Name = name;
			this.iList = iList;
		}

		public ListSeries(IList iList, PropertyInfo propertyInfo)
		{
			this.iList = iList;
			this.xPropertyInfo = propertyInfo;

			Name = propertyInfo.Name;
			Name = Name.AddSpacesBetweenWords();
			NameAttribute attribute = propertyInfo.GetCustomAttribute<NameAttribute>();
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
