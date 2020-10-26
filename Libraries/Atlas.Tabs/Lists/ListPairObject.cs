using Atlas.Core;
using System;
using System.ComponentModel;

namespace Atlas.Tabs
{
	// implement INotifyPropertyChanged to prevent memory leaks
	public class ListPair : IListPair, IListItem, INotifyPropertyChanged, IMaxDesiredWidth
	{
		[StyleLabel]
		public object Key { get; set; }
		[StyleValue]
		public object Value { get; set; }
		[HiddenColumn, InnerValue]
		public object Object { get; set; }
		public bool AutoLoad = true;

		[HiddenColumn]
		public int? MaxDesiredWidth { get; set; }

#pragma warning disable 414
		public event PropertyChangedEventHandler PropertyChanged = null;

		public ListPair(object key, object value, object obj = null, int? maxDesiredWidth = null)
		{
			Key = key;
			Value = value;
			if (obj != null)
				Object = obj;
			else
				Object = value;
			MaxDesiredWidth = maxDesiredWidth;
		}

		public override string ToString()
		{
			if (Key != null)
			{
				string description = Key.ToString();
				if (description != null)
					return description;
			}

			return "";
		}

		// DataGrid columns bind to this
		/*public string Name
		{
			get
			{
				return Key.Formatted();
			}
			set
			{
				Key = value;
			}
		}*/
	}
}
