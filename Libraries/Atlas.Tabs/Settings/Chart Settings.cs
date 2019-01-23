using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.Tabs
{
	public class ChartSettings
	{
		public string Name { get; set; }
		private ListGroup DefaultListGroup { get; set; } = new ListGroup("Default");
		public Dictionary<string, ListGroup> ListGroups { get; set; } = new Dictionary<string, ListGroup>();
		//public ItemCollection<ListGroup> ListGroups { get; set; } = new ItemCollection<ListGroup>();
		public ItemCollection<ListSeries> ListSeries { get; set; } = new ItemCollection<ListSeries>();
		
		public override string ToString()
		{
			return String.Join(" ", ListSeries);
			//return ListSeries[0].ToString(); // todo: fix for multiple
		}

		public ChartSettings()
		{

		}

		public ChartSettings(IList iList)
		{
			Type type = iList.GetType();
			Type elementType = null;
			if (iList is Array)
				elementType = type.GetElementType();
			else //if (type.GenericTypeArguments.Length > 0)
				elementType = type.GenericTypeArguments[0];
			PropertyInfo[] properties = elementType.GetProperties().OrderBy(x => x.MetadataToken).ToArray();
			ItemCollection<ListSeries> listProperties = new ItemCollection<ListSeries>();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.DeclaringType.IsNotPublic)
					continue;
				if (propertyInfo.PropertyType.IsNumeric())
				{
					ListSeries listSeries = new ListSeries(propertyInfo);
					listSeries.iList = iList;
					listProperties.Add(listSeries);

					ListGroup listGroup = DefaultListGroup;
					UnitAttribute attribute = propertyInfo.GetCustomAttribute(typeof(UnitAttribute)) as UnitAttribute;
					if (attribute != null)
					{
						if (!ListGroups.TryGetValue(attribute.Name, out listGroup))
						{
							listGroup = new ListGroup(attribute.Name);
							ListGroups[attribute.Name] = listGroup;
						}
					}
					// Will add to Default Group if no Unit specified, and add the Default Group if needed
					listGroup.ListSeries.Add(listSeries);
					this.ListSeries.Add(listSeries);
				}
			}
		}

		// todo: add Append?
	}
}