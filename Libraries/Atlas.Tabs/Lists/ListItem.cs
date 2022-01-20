using Atlas.Core;
using Atlas.Extensions;
using System.Collections.Generic;
using System.ComponentModel;

namespace Atlas.Tabs;

public interface IListItem
{
	[Name("Name")]
	object Key { get; }

	[HiddenColumn, InnerValue, StyleValue]
	object Value { get; set; }
}

// implement INotifyPropertyChanged to prevent memory leaks
public class ListItem : IListItem, INotifyPropertyChanged
{
	[HiddenColumn]
	public object Key { get; set; }

	[HiddenColumn, InnerValue]
	public object Value { get; set; }

	// DataGrid columns bind to this
	public string Name
	{
		get => Key?.Formatted();
		set => Key = value;
	}

	public bool AutoLoad = true;

#pragma warning disable 414
	public event PropertyChangedEventHandler PropertyChanged = null;

	public override string ToString() => Key?.ToString() ?? "";

	public ListItem(object key, object value)
	{
		Key = key;
		Value = value;
	}

	// todo: move into IListItem after upgrading to .Net Standard 2.1
	// Get list items for all public properties and any methods marked with [Item]
	public static ItemCollection<IListItem> Create(object obj, bool includeBaseTypes)
	{
		var listItems = new SortedDictionary<int, IListItem>();

		var properties = ListProperty.Create(obj, includeBaseTypes);
		foreach (ListProperty listProperty in properties)
		{
			int metadataToken = listProperty.PropertyInfo.GetGetMethod(false).MetadataToken;

			listItems.Add(metadataToken, listProperty);
		}

		var methods = ListMethod.Create(obj, includeBaseTypes);
		foreach (ListMethod listMethod in methods)
		{
			listItems.Add(listMethod.MethodInfo.MetadataToken, listMethod);
		}

		return new ItemCollection<IListItem>(listItems.Values);
	}
}
