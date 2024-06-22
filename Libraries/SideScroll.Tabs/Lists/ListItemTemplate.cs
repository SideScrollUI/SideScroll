using SideScroll;
using System.ComponentModel;

namespace SideScroll.Tabs;

// implement INotifyPropertyChanged to prevent memory leaks
public class ListItem<TKey, TValue>(TKey key, TValue value, object obj) : INotifyPropertyChanged
{
	public TKey Key { get; set; } = key;
	public TValue Value { get; set; } = value;

	[HiddenColumn, InnerValue]
	public object Object { get; set; } = obj;

	public bool AutoLoad = true;
#pragma warning disable 414
	public event PropertyChangedEventHandler? PropertyChanged;

	public override string ToString() => Key?.ToString() ?? "";
}

/*
Not used anywhere. Remove?
Uses:
	Passing dynamic types?
	Setting return type?
*/
