using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Atlas.Core;
using Atlas.Extensions;

namespace Atlas.Tabs
{
	public enum SelectionType
	{
		None,
		User,
		Auto,
	}

	public class TabDataSettings
	{
		public HashSet<SelectedRow> SelectedRows { get; set; } = new HashSet<SelectedRow>(); // needs to be nullable or we need another initialized value
		public SelectionType SelectionType { get; set; } = SelectionType.None;
		public List<string> ColumnNameOrder { get; set; } = new List<string>(); // Order to show the columns in, users can drag columns around to reorder these
		public int TotalColumns { get; set; } // unused, use to detect changes?

		public string SortColumnName { get; set; } // Currently sorted column
		public ListSortDirection SortDirection { get; set; }

		public string Filter { get; set; }

		public static List<MethodColumn> GetButtonMethods(Type type)
		{
			List<MethodColumn> callableMethods = new List<MethodColumn>();
			MethodInfo[] methodInfos = type.GetMethods().OrderBy(x => x.MetadataToken).ToArray();
			foreach (MethodInfo methodInfo in methodInfos)
			{
				var attribute = methodInfo.GetCustomAttribute<ButtonColumnAttribute>();
				if (attribute == null)
					continue;

				callableMethods.Add(new MethodColumn(methodInfo, attribute.Name ?? methodInfo.Name));
			}
			return callableMethods;
		}

		public static List<PropertyInfo> GetVisibleProperties(Type type)
		{
			var visibleProperties = new List<PropertyInfo>();
			// Properties are returned in a random order, so sort them by the MetadataToken to get the original order
			PropertyInfo[] propertyInfos = type.GetProperties().OrderBy(x => x.MetadataToken).ToArray();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (propertyInfo.GetCustomAttribute<HiddenColumnAttribute>() != null)
					continue;

				visibleProperties.Add(propertyInfo);
			}
			return visibleProperties;
		}

		public class MethodColumn
		{
			public MethodInfo methodInfo;
			public string label;

			public MethodColumn(MethodInfo methodInfo, string label)
			{
				this.methodInfo = methodInfo;
				this.label = label;
			}
		}

		public class PropertyColumn
		{
			public PropertyInfo propertyInfo;
			public string label;

			public PropertyColumn(PropertyInfo propertyInfo, string label)
			{
				this.propertyInfo = propertyInfo;
				this.label = label;
			}
		}

		private List<PropertyInfo> GetPropertyColumns(Type elementType)
		{
			List<PropertyInfo> visibleProperties = GetVisibleProperties(elementType);

			if (ColumnNameOrder?.Count > 0)
			{
				Dictionary<string, PropertyInfo> propertyNames = new Dictionary<string, PropertyInfo>();
				foreach (PropertyInfo propertyInfo in visibleProperties)
					propertyNames[propertyInfo.Name] = propertyInfo;

				// Add all previously seen property infos
				var orderedPropertyInfos = new List<PropertyInfo>();
				foreach (string columnName in ColumnNameOrder)
				{
					PropertyInfo propertyInfo;
					if (propertyNames.TryGetValue(columnName, out propertyInfo))
					{
						orderedPropertyInfos.Add(propertyInfo);
						propertyNames.Remove(columnName);
					}
				}
				// todo: fix random order since Dictionary Reorders
				orderedPropertyInfos.AddRange(propertyNames.Values);
				return orderedPropertyInfos;
			}
			return visibleProperties;
		}

		public List<PropertyColumn> GetPropertiesAsColumns(Type elementType)
		{
			List<PropertyInfo> visibleProperties = GetPropertyColumns(elementType);

			var propertyColumns = new List<PropertyColumn>();

			// Properties are returned in a random order, so sort them by the MetadataToken to get the original order
			//Debug.Assert(visibleProperties.Count > 0); // built in types don't always have properties
			foreach (PropertyInfo propertyInfo in visibleProperties)
			{
				string label;
				NameAttribute attribute = propertyInfo.GetCustomAttribute<NameAttribute>();
				if (attribute != null)
				{
					label = attribute.Name;
				}
				else
				{
					label = propertyInfo.Name.WordSpaced();
				}
				propertyColumns.Add(new PropertyColumn(propertyInfo, label));
			}
			return propertyColumns;
		}
	}

	public class SelectedRow
	{
		public string label; // null if ToString() returns type
		public int rowIndex;
		[NonSerialized]
		public object obj; // used for bookmark searches, dangerous to keep these references around otherwise
		public string dataKey;
		public object dataValue;
		//public bool pinned;
		public List<string> selectedColumns = new List<string>();

		public override string ToString()
		{
			return label;
		}
	}
}
/*
Type of control
Name of control
	Usually a reference
*/