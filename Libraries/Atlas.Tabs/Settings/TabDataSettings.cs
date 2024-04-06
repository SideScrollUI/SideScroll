using Atlas.Core;
using Atlas.Extensions;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Atlas.Tabs;

public enum SelectionType
{
	None,
	User,
	Auto,
}

[PublicData]
public class TabDataSettings
{
	public HashSet<SelectedRow> SelectedRows { get; set; } = new(); // needs to be nullable or we need another initialized value
	public SelectionType SelectionType { get; set; } = SelectionType.None;
	public List<string> ColumnNameOrder { get; set; } = new(); // Order to show the columns in, users can drag columns around to reorder these
	public int TotalColumns { get; set; } // unused, use to detect changes?

	public string? SortColumnName { get; set; } // Currently sorted column
	public ListSortDirection SortDirection { get; set; }

	public string? Filter { get; set; }
	public string? Address
	{
		get
		{
			var labels = SelectedRows.Select(s => s.Label).ToList();
			if (labels.Count <= 1)
				return labels.FirstOrDefault();

			return "[" + string.Join(", ", labels) + "] ";
		}
	}

	public override string? ToString() => Address;

	public static List<MethodColumn> GetButtonMethods(Type type)
	{
		var callableMethods = new List<MethodColumn>();
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

	public static List<PropertyInfo> GetVisibleElementProperties(IList list)
	{
		Type listType = list.GetType();
		Type elementType = listType.GetGenericArguments()[0]; // dictionaries?
		return GetVisibleProperties(elementType);
	}

	private static readonly Dictionary<Type, List<PropertyInfo>> _visiblePropertiesCache = new();

	public static List<PropertyInfo> GetVisibleProperties(Type type)
	{
		lock (_visiblePropertiesCache)
		{
			if (_visiblePropertiesCache.TryGetValue(type, out List<PropertyInfo>? list))
				return list;

			list = type.GetVisibleProperties();
			_visiblePropertiesCache.Add(type, list);
			return list;
		}
	}

	public class MethodColumn(MethodInfo methodInfo, string? label = null)
	{
		public readonly MethodInfo MethodInfo = methodInfo;
		public string Label { get; set; } = label ?? methodInfo.GetCustomAttribute<ButtonColumnAttribute>()?.Name ?? methodInfo.Name;
	}

	public class PropertyColumn(PropertyInfo propertyInfo, string label)
	{
		public readonly PropertyInfo PropertyInfo = propertyInfo;
		public string Label { get; set; } = label;

		public override string ToString() => Label;

		public bool IsStyled()
		{
			return PropertyInfo.IsDefined(typeof(StyleValueAttribute)) ||
				typeof(DictionaryEntry).IsAssignableFrom(PropertyInfo.DeclaringType);
		}

		public bool IsVisible(IList list)
		{
			if (PropertyInfo.GetCustomAttribute<HideAttribute>() == null &&
				PropertyInfo.GetCustomAttribute<HideColumnAttribute>() == null ||
				list.Count == 0)
				return true;

			foreach (object obj in list)
			{
				try
				{
					var listProperty = new ListProperty(obj, PropertyInfo);
					if (listProperty.IsColumnVisible())
						return true;
				}
				catch (Exception)
				{
				}
			}
			return false;
		}
	}

	private List<PropertyInfo> GetPropertyColumns(Type elementType)
	{
		List<PropertyInfo> visibleProperties = GetVisibleProperties(elementType);

		if (ColumnNameOrder?.Count > 0)
		{
			var propertyNames = new Dictionary<string, PropertyInfo>();
			foreach (PropertyInfo propertyInfo in visibleProperties)
				propertyNames[propertyInfo.Name] = propertyInfo;

			// Add all previously seen property infos
			var orderedPropertyInfos = new List<PropertyInfo>();
			foreach (string columnName in ColumnNameOrder)
			{
				if (propertyNames.TryGetValue(columnName, out PropertyInfo? propertyInfo))
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
			NameAttribute? attribute = propertyInfo.GetCustomAttribute<NameAttribute>();
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
