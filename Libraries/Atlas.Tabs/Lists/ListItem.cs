using Atlas.Core;
using Atlas.Extensions;
using System.ComponentModel;

namespace Atlas.Tabs;

public interface IListItem
{
	[Name("Name")]
	object? Key { get; }

	[HiddenColumn, InnerValue, StyleValue]
	object? Value { get; }

	// Get list items for all public properties and any methods marked with [Item]
	public static ItemCollection<IListItem> Create(object obj, bool includeBaseTypes)
	{
		var listItems = new SortedDictionary<int, IListItem>();

		var properties = ListProperty.Create(obj, includeBaseTypes);
		foreach (ListProperty listProperty in properties)
		{
			int metadataToken = listProperty.PropertyInfo.GetGetMethod(false)!.MetadataToken;

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

// implement INotifyPropertyChanged to prevent memory leaks
public class ListItem(object? key, object? value) : IListItem, INotifyPropertyChanged
{
	[HiddenColumn]
	public object? Key { get; set; } = key;

	[HiddenColumn, InnerValue]
	public object? Value { get; set; } = value;

	// DataGrid columns bind to this
	public string Name
	{
		get => Key.Formatted()!;
		set => Key = value;
	}

	public bool AutoLoad = true;

#pragma warning disable 414
	public event PropertyChangedEventHandler? PropertyChanged;

	public override string ToString() => Key?.ToString() ?? "";
}
