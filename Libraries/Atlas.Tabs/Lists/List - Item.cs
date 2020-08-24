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
		public bool autoLoad = true;

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

		// Get list items for all public properties and any methods marked with [Item]
		// todo: need lazy version
		public static List<ListItem> Create(object obj, bool includeBaseTypes)
		{
			var listItems = new SortedDictionary<int, ListItem>();

			var properties = ListProperty.Create(obj);
			foreach (ListProperty listProperty in properties)
			{
				if (!includeBaseTypes && listProperty.propertyInfo.DeclaringType != obj.GetType())
					continue;
				string name = listProperty.Name;
				// Only show [DebugOnly] in debug mode
				if (listProperty.propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				{
#if !DEBUG
					continue;
#endif
					name = "*" + name;
				}

				int metadataToken = listProperty.propertyInfo.GetGetMethod(false).MetadataToken;
				object value = listProperty.Value;

				if (listProperty.propertyInfo.GetCustomAttribute<HideNullAttribute>() != null && value == null)
					continue;

				listItems.Add(metadataToken, new ListItem(name, value));
			}

			var methods = ListMethod.Create(obj);
			foreach (ListMethod listMethod in methods)
			{
				if (!includeBaseTypes && listMethod.methodInfo.DeclaringType != obj.GetType())
					continue;

				listItems.Add(listMethod.methodInfo.MetadataToken, new ListItem(listMethod.Name, listMethod.Value));
			}

			return listItems.Values.ToList();
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
