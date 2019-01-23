using System;
using System.Collections;
using System.ComponentModel;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	// implement INotifyPropertyChanged to prevent memory leaks
	public class ListPair : INotifyPropertyChanged
	{
		public object Name { get; set; }
		public object Value { get; set; }
		[HiddenColumn]
		[InnerValue]
		public object Object { get; set; }
		public bool autoLoad = true;

#pragma warning disable 414
		public event PropertyChangedEventHandler PropertyChanged = null;

		public ListPair(object key, object value, object obj = null)
		{
			this.Name = key;
			this.Value = value;
			if (obj != null)
				this.Object = obj;
			else
				this.Object = value;
		}

		public override string ToString()
		{
			if (Name != null)
			{
				string description = Name.ToString();
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