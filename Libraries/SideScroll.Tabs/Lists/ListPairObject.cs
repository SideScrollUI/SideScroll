using SideScroll.Attributes;
using System.ComponentModel;

namespace SideScroll.Tabs.Lists;

// implement INotifyPropertyChanged to prevent memory leaks
public class ListPair(object key, object? value, object? obj = null, int? maxDesiredWidth = null, int? maxDesiredHeight = null)
	: IListPair, IListItem, INotifyPropertyChanged, IMaxDesiredWidth, IMaxDesiredHeight
{
	public object Key { get; set; } = key;

	[StyleValue]
	public object? Value { get; set; } = value;

	[HiddenColumn, InnerValue]
	public object? Object { get; set; } = obj ?? value;

	[HiddenColumn]
	public bool IsAutoSelectable { get; set; } = true;

	[HiddenColumn]
	public int? MaxDesiredWidth { get; set; } = maxDesiredWidth;

	[HiddenColumn]
	public int? MaxDesiredHeight { get; set; } = maxDesiredHeight;

#pragma warning disable 414
	public event PropertyChangedEventHandler? PropertyChanged;

	public override string ToString() => Key?.ToString() ?? "";
}
