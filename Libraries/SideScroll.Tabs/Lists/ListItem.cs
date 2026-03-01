using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using System.ComponentModel;
using System.Reflection;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Interface for list items that can be displayed in DataGrids with a key-value structure
/// </summary>
public interface IListItem
{
	/// <summary>
	/// Gets the key/name of the item
	/// </summary>
	[Name("Name")]
	object? Key { get; }

	/// <summary>
	/// Gets the value of the item
	/// </summary>
	[HiddenColumn, InnerValue, StyleValue]
	object? Value { get; }

	/// <summary>
	/// Gets whether the item can be auto-selected in UI
	/// </summary>
	[HiddenColumn]
	bool IsAutoSelectable { get; }

	/// <summary>
	/// Creates list items for all public properties and methods marked with [Item] from an object
	/// </summary>
	/// <param name="obj">The object to extract items from</param>
	/// <param name="includeBaseTypes">Whether to include members from base types</param>
	public static ItemCollection<IListItem> Create(object obj, bool includeBaseTypes)
	{
		var listItems = new SortedDictionary<string, IListItem>();

		var properties = ListProperty.Create(obj, includeBaseTypes);
		foreach (ListProperty listProperty in properties)
		{
			MethodInfo getMethod = listProperty.PropertyInfo.GetGetMethod(false)!;
			string id = $"{getMethod.Module.Name}:{getMethod.MetadataToken:D10}";
			listItems.Add(id, listProperty);
		}

		var methods = ListMethod.Create(obj, includeBaseTypes);
		foreach (ListMethod listMethod in methods)
		{
			string id = $"{listMethod.MethodInfo.Module.Name}:{listMethod.MethodInfo.MetadataToken:D10}";
			listItems.Add(id, listMethod);
		}

		return new ItemCollection<IListItem>(listItems.Values);
	}
}

/// <summary>
/// Represents a simple key-value list item with property change notification support
/// </summary>
public class ListItem(object? key, object? value) : IListItem, INotifyPropertyChanged
{
	/// <summary>
	/// Gets or sets the key of the item
	/// </summary>
	[HiddenColumn]
	public object? Key { get; set; } = key;

	/// <summary>
	/// Gets or sets the value of the item
	/// </summary>
	[HiddenColumn, InnerValue]
	public object? Value { get; set; } = value;

	/// <summary>
	/// Gets or sets the formatted name for display in DataGrid columns
	/// </summary>
	public string Name
	{
		get => Key.Formatted()!;
		set => Key = value;
	}

	/// <summary>
	/// Gets or sets whether the item can be auto-selected
	/// </summary>
	[HiddenColumn]
	public bool IsAutoSelectable { get; set; } = true;

#pragma warning disable 414
	/// <summary>
	/// Event raised when a property value changes
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	public override string ToString() => Key?.ToString() ?? "";
}
