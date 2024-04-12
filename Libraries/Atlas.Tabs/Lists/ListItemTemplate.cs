using Atlas.Core;
using System.ComponentModel;

namespace Atlas.Tabs;

// implement INotifyPropertyChanged to prevent memory leaks
public class ListItem<T1, T2>(T1 key, T2 value, object obj) : INotifyPropertyChanged
{
	public T1 Key { get; set; } = key;
	public T2 Value { get; set; } = value;

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
