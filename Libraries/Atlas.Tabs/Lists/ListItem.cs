using Atlas.Core;
using Atlas.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Atlas.Tabs
{
	// implement INotifyPropertyChanged to prevent memory leaks
	public class ListItem : IListItem, INotifyPropertyChanged
	{
		[HiddenColumn]
		public object Key { get; set; }
		[HiddenColumn, InnerValue]
		public object Value { get; set; }
		public bool AutoLoad = true;

#pragma warning disable 414
		public event PropertyChangedEventHandler PropertyChanged = null;

		public ListItem(object key, object value)
		{
			Key = key;
			Value = value;
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
				return Key.Formatted();
			}
			set
			{
				Key = value;
			}
		}

		// todo: move into IListItem after upgrading to .Net Standard 2.1
		// Get list items for all public properties and any methods marked with [Item]
		public static ItemCollection<IListItem> Create(object obj, bool includeBaseTypes)
		{
			var listItems = new SortedDictionary<int, IListItem>();

			var properties = ListProperty.Create(obj);
			foreach (ListProperty listProperty in properties)
			{
				if (!includeBaseTypes && listProperty.PropertyInfo.DeclaringType != obj.GetType())
					continue;
#if !DEBUG
				// Only show [DebugOnly] in debug mode
				if (listProperty.PropertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
					continue;
#endif

				int metadataToken = listProperty.PropertyInfo.GetGetMethod(false).MetadataToken;

				listItems.Add(metadataToken, listProperty);
			}

			var methods = ListMethod.Create(obj);
			foreach (ListMethod listMethod in methods)
			{
				if (!includeBaseTypes && listMethod.MethodInfo.DeclaringType != obj.GetType())
					continue;

				listItems.Add(listMethod.MethodInfo.MetadataToken, listMethod);
			}

			return new ItemCollection<IListItem>(listItems.Values);
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
