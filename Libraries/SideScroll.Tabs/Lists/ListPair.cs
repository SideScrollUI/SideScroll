using SideScroll.Attributes;
using System.ComponentModel;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Represents a key-value pair with optional object reference and size constraints, implementing INotifyPropertyChanged to prevent memory leaks
/// </summary>
public class ListPair(object key, object? value, object? obj = null, int? maxDesiredWidth = null, int? maxDesiredHeight = null)
	: IListPair, IListItem, INotifyPropertyChanged, IMaxDesiredWidth, IMaxDesiredHeight
{
	/// <summary>
	/// Gets or sets the key of the pair
	/// </summary>
	public object Key { get; set; } = key;

	/// <summary>
	/// Gets or sets the value of the pair
	/// </summary>
	[StyleValue]
	public object? Value { get; set; } = value;

	/// <summary>
	/// Gets or sets the underlying object (defaults to Value if not specified)
	/// </summary>
	[HiddenColumn, InnerValue]
	public object? Object { get; set; } = obj ?? value;

	/// <summary>
	/// Gets or sets whether the item can be auto-selected
	/// </summary>
	[HiddenColumn]
	public bool IsAutoSelectable { get; set; } = true;

	/// <summary>
	/// Gets or sets the maximum desired width in pixels
	/// </summary>
	[HiddenColumn]
	public int? MaxDesiredWidth { get; set; } = maxDesiredWidth;

	/// <summary>
	/// Gets or sets the maximum desired height in pixels
	/// </summary>
	[HiddenColumn]
	public int? MaxDesiredHeight { get; set; } = maxDesiredHeight;

#pragma warning disable 414
	/// <summary>
	/// Event raised when a property value changes
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	public override string ToString() => Key?.ToString() ?? "";
}

/// <summary>
/// Generic key-value pair with strong typing, implementing INotifyPropertyChanged to prevent memory leaks
/// </summary>
/// <typeparam name="TKey">The type of the key</typeparam>
/// <typeparam name="TValue">The type of the value</typeparam>
public class ListPair<TKey, TValue>(TKey key, TValue value, object? obj = null) : INotifyPropertyChanged
{
	/// <summary>
	/// Gets or sets the key of the pair
	/// </summary>
	public TKey Key { get; set; } = key;

	/// <summary>
	/// Gets or sets the value of the pair
	/// </summary>
	public TValue Value { get; set; } = value;

	/// <summary>
	/// Gets or sets the underlying object (defaults to Value if not specified)
	/// </summary>
	[HiddenColumn, InnerValue]
	public object? Object { get; set; } = obj ?? value;

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
