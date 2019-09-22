using System;
using System.Collections;
using System.ComponentModel;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	// implement INotifyPropertyChanged to prevent memory leaks
	public class ListItem : IListItem, INotifyPropertyChanged
	{
		[HiddenColumn]
		public object Key { get; set; }
		[HiddenColumn, InnerValue]
		public object Value { get; set; }
		public bool autoLoad = true;

#pragma warning disable 414
		public event PropertyChangedEventHandler PropertyChanged = null;

		public ListItem(object key, object value)
		{
			this.Key = key;
			this.Value = value;
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
		public string Name
		{
			get
			{
				return Key.ObjectToString();
			}
			set
			{
				this.Key = value;
			}
		}
	}

	public interface IListItem
	{
		[Name("Name")]
		object Key { get; }

		[HiddenColumn, InnerValue, StyleValue]
		object Value { get; set; }
	}
}
