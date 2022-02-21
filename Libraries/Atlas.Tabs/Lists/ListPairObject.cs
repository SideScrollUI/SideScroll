using Atlas.Core;
using System;
using System.ComponentModel;

namespace Atlas.Tabs;

// implement INotifyPropertyChanged to prevent memory leaks
public class ListPair : IListPair, IListItem, INotifyPropertyChanged, IMaxDesiredWidth
{
	[StyleLabel]
	public object Key { get; set; }

	[StyleValue]
	public object Value { get; set; }

	[HiddenColumn, InnerValue]
	public object Object { get; set; }

	// public bool AutoLoad = true;

	[HiddenColumn]
	public int? MaxDesiredWidth { get; set; }

#pragma warning disable 414
	public event PropertyChangedEventHandler PropertyChanged;

	public override string ToString() => Key?.ToString() ?? "";

	public ListPair(object key, object value, object obj = null, int? maxDesiredWidth = null)
	{
		Key = key;
		Value = value;
		Object = obj ?? value;
		MaxDesiredWidth = maxDesiredWidth;
	}
}
