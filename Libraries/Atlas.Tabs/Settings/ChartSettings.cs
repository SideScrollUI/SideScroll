using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.Tabs;

// Call ChartGroupControl.Register() to register UI Control
public class ChartSettings
{
	private const string DefaultGroupName = "Default";

	public string Name { get; set; }
	private ListGroup DefaultListGroup { get; set; } = new(DefaultGroupName);
	public Dictionary<string, ListGroup> ListGroups { get; set; } = new();
	//public ItemCollection<ListGroup> ListGroups { get; set; } = new();
	public ItemCollection<ListSeries> ListSeries { get; set; } = new();

	public override string ToString() => string.Join(" ", ListSeries);

	public ChartSettings()
	{
	}

	public ChartSettings(ListGroup listGroup)
	{
		AddGroup(listGroup);
	}

	public ChartSettings(ListSeries listSeries, string name = null)
	{
		Name = name;
		DefaultListGroup.Name = name ?? DefaultListGroup.Name;

		AddSeries(listSeries);
	}

	public ChartSettings(IList iList, string name = null)
	{
		Name = name;
		DefaultListGroup.Name = name ?? DefaultListGroup.Name;
		LoadList(iList);
	}

	public void LoadList(IList iList)
	{
		Type type = iList.GetType();
		Type elementType = null;
		if (iList is Array)
			elementType = type.GetElementType();
		else //if (type.GenericTypeArguments.Length > 0)
			elementType = type.GenericTypeArguments[0];

		if (elementType.IsPrimitive)
		{
			AddList("Values", iList);
			return;
		}

		PropertyInfo xAxisPropertyInfo = elementType.GetPropertyWithAttribute<XAxisAttribute>();

		PropertyInfo[] properties = elementType.GetProperties()
			.OrderBy(x => x.MetadataToken)
			.ToArray();

		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.DeclaringType.IsNotPublic)
				continue;

			if (propertyInfo.PropertyType.IsNumeric())
			{
				var listSeries = new ListSeries(iList, xAxisPropertyInfo, propertyInfo);
				//listProperties.Add(listSeries);

				ListGroup listGroup = DefaultListGroup;
				UnitAttribute attribute = propertyInfo.GetCustomAttribute<UnitAttribute>();
				if (attribute != null)
				{
					if (!ListGroups.TryGetValue(attribute.Name, out listGroup))
					{
						listGroup = new ListGroup(attribute.Name);
						ListGroups[attribute.Name] = listGroup;
					}
				}
				else
				{
					if (!ListGroups.ContainsKey(listGroup.Name))
					{
						ListGroups[listGroup.Name] = listGroup;
					}
				}
				// Will add to Default Group if no Unit specified, and add the Default Group if needed
				listGroup.Series.Add(listSeries);
				ListSeries.Add(listSeries);
			}
		}
	}

	// todo: this needs to be reworked when a use is found
	public void AddList(string label, IList iList)
	{
		var listSeries = new ListSeries(label, iList);

		//if (ListGroups.TryGetValue(label, out ListGroup listGroup)

		ListGroup listGroup = DefaultListGroup;
		listGroup.Name = label ?? listGroup.Name;
		// Will add to Default Group if no Unit specified, and add the Default Group if needed
		ListGroups.Add(listGroup.Name, listGroup);
		listGroup.Series.Add(listSeries);
		ListSeries.Add(listSeries);
	}

	public void AddGroup(ListGroup listGroup)
	{
		ListGroups.Add(listGroup.Name, listGroup);
		ListSeries.AddRange(listGroup.Series);
	}

	public void AddSeries(ListSeries listSeries)
	{
		ListGroup listGroup = DefaultListGroup;
		if (listGroup.Name == DefaultGroupName)
			listGroup.Name = listSeries.Name ?? listGroup.Name;

		// Will add to Default Group if no Unit specified, and add the Default Group if needed
		ListGroups.Add(listGroup.Name, listGroup);
		listGroup.Series.Add(listSeries);
		ListSeries.Add(listSeries);
	}

	public void SetTimeWindow(TimeWindow timeWindow, bool showTimeTracker)
	{
		foreach (ListGroup listGroup in ListGroups.Values)
		{
			listGroup.TimeWindow = timeWindow;
			listGroup.ShowTimeTracker = showTimeTracker;
		}
	}

	// todo: add Append?
}
