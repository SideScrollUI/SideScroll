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
		public ItemCollection<ListSeries> ListSeries { get; set; } = new ItemCollection<ListSeries>();
		public ListGroup(string name)
		{
			Name = name;
		}
	}

	public class ListSeries
	{
		public string Name { get; set; }
		public IList iList; // List to start with, any elements added will also trigger an event to add new points
		public PropertyInfo propertyInfo; // optional
		//public object obj;
		
		public ListSeries(string name, IList iList)
		{
			Name = name;
			this.iList = iList;
		}

		public ListSeries(PropertyInfo propertyInfo)
		{
			this.propertyInfo = propertyInfo;

			Name = propertyInfo.Name;
			Name = Name.AddSpacesBetweenWords();
			NameAttribute attribute = propertyInfo.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
			if (attribute != null)
				Name = attribute.Name;
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
