using Atlas.Core;
using System.ComponentModel;

namespace Atlas.Tabs
{
	// implement INotifyPropertyChanged to prevent memory leaks
	public class ListItem<T1, T2> : INotifyPropertyChanged
	{
		public T1 Key { get; set; }
		public T2 Value { get; set; }

		[HiddenColumn, InnerValue]
		public object Object { get; set; }

		public bool AutoLoad = true;
#pragma warning disable 414
		public event PropertyChangedEventHandler PropertyChanged = null;

		public override string ToString() => Key?.ToString() ?? "";

		public ListItem(T1 key, T2 value, object obj)
		{
			Key = key;
			Value = value;
			Object = obj;
		}
	}
}

/*
Not used anywhere. Remove?
Uses:
	Passing dynamic types?
	Setting return type?
*/
