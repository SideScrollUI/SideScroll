using SideScroll.Attributes;
using System.ComponentModel;

namespace SideScroll.Tabs.Lists;

// implement INotifyPropertyChanged to prevent memory leaks
public class ListPair(object key, object? value, object? obj = null, int? maxDesiredWidth = null)
	: IListPair, IListItem, INotifyPropertyChanged, IMaxDesiredWidth
{
	public object Key { get; set; } = key;

	[StyleValue]
	public object? Value { get; set; } = value;

	[HiddenColumn, InnerValue]
	public object? Object { get; set; } = obj ?? value;

	// public bool AutoLoad = true;

	[HiddenColumn]
	public int? MaxDesiredWidth { get; set; } = maxDesiredWidth;

#pragma warning disable 414
	public event PropertyChangedEventHandler? PropertyChanged;

	public override string ToString() => Key?.ToString() ?? "";
}
