using System;
using System.Collections;
using System.ComponentModel;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	// implement INotifyPropertyChanged to prevent memory leaks
	public class ListItem : INotifyPropertyChanged
	{
		public object key;
		[HiddenColumn]
		[InnerValue]
		public object Value { get; set; }
		public bool autoLoad = true;

#pragma warning disable 414
		public event PropertyChangedEventHandler PropertyChanged = null;

		public ListItem(object key, object value)
		{
			this.key = key;
			this.Value = value;
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
		
		// DataGrid columns bind to this
		public string Name
		{
			get
			{
				return key.ObjectToString();
			}
			set
			{
				this.key = value;
			}
		}
	}
}

/*
*/