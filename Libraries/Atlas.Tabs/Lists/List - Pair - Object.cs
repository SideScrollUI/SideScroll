using System;
using System.Collections;
using System.ComponentModel;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	// implement INotifyPropertyChanged to prevent memory leaks
	public class ListPair : IListPair, IListItem, INotifyPropertyChanged, IMaxDesiredWidth
	{
		public object Key { get; set; }
		[StyleValue]
		public object Value { get; set; }
		[HiddenColumn]
		[InnerValue]
		public object Object { get; set; }
		public bool autoLoad = true;

		[HiddenColumn]
		public int? MaxDesiredWidth { get; set; }

#pragma warning disable 414
		public event PropertyChangedEventHandler PropertyChanged = null;

		public ListPair(object key, object value, object obj = null, int? maxDesiredWidth = null)
		{
			this.Key = key;
			this.Value = value;
			if (obj != null)
				this.Object = obj;
			else
				this.Object = value;
			this.MaxDesiredWidth = maxDesiredWidth;
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
				return key.ObjectToString();
			}
			set
			{
				this.key = value;
			}
		}*/
	}
}

/*
*/