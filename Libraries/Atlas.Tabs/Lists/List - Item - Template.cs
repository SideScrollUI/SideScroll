using System.ComponentModel;
using Atlas.Core;

namespace Atlas.Tabs
{
	// implement INotifyPropertyChanged to prevent memory leaks
	public class ListItem<T1, T2> : INotifyPropertyChanged
	{
		public T1 key { get; set; }
		public T2 Value { get; set; }

		[HiddenColumn]
		[InnerValue]
		public object obj { get; set; }

		public bool autoLoad = true;
#pragma warning disable 414
		public event PropertyChangedEventHandler PropertyChanged = null;

		public ListItem(T1 key, T2 value, object obj)
		{
			this.key = key;
			this.Value = value;
			this.obj = obj;
		}

		public override string ToString()
		{
			if (key != null)
			{
				string description = key.ToString();
				if (description != null)
					return description;
			}

			return "";
		}
	}
}

/*
Not used anywhere. Remove?
*/