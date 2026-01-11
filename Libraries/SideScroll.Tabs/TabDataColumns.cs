using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.Reflection;

namespace SideScroll.Tabs;

public class TabDataColumns(List<string>? columnNameOrder = null)
{
	// Order to show the columns in, users can drag columns around to reorder these
	public List<string> ColumnNameOrder { get; set; } = columnNameOrder ?? [];

	private static readonly Dictionary<Type, List<PropertyInfo>> _visiblePropertiesCache = [];

	public static List<TabMethodColumn> GetMethodColumns(Type type)
	{
		var methodInfos = type.GetMethods()
			.OrderBy(m => m.Module.Name)
			.ThenBy(m => m.MetadataToken);

		List<TabMethodColumn> methodColumns = [];
		foreach (MethodInfo methodInfo in methodInfos)
		{
			var attribute = methodInfo.GetCustomAttribute<ButtonColumnAttribute>();
			if (attribute == null)
				continue;

			methodColumns.Add(new TabMethodColumn(methodInfo, attribute.Name ?? methodInfo.Name));
		}
		return methodColumns;
	}

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

	public static List<PropertyInfo> GetVisibleElementProperties(IList list)
	{
		Type listType = list.GetType();
		Type elementType = listType.GetGenericArguments()[0]; // Dictionaries?
		return GetVisibleProperties(elementType);
	}

	private List<PropertyInfo> GetOrderedPropertyColumns(Type elementType)
	{
		List<PropertyInfo> visibleProperties = GetVisibleProperties(elementType);
		if (ColumnNameOrder?.Count > 0)
		{
			var propertyNames = visibleProperties.ToDictionary(propertyInfo => propertyInfo.Name);

			// Add all previously seen property infos
			List<PropertyInfo> orderedPropertyInfos = [];
			foreach (string columnName in ColumnNameOrder)
			{
				if (propertyNames.TryGetValue(columnName, out PropertyInfo? propertyInfo))
				{
					orderedPropertyInfos.Add(propertyInfo);
					propertyNames.Remove(columnName);
				}
			}
			// todo: Fix random order since Dictionary Reorders
			orderedPropertyInfos.AddRange(propertyNames.Values);
			return orderedPropertyInfos;
		}
		return visibleProperties;
	}

	public List<TabPropertyColumn> GetPropertyColumns(Type elementType)
	{
		List<PropertyInfo> visibleProperties = GetOrderedPropertyColumns(elementType);

		List<TabPropertyColumn> propertyColumns = [];

		// Properties are returned in a random order, so sort them by the MetadataToken to get the original order
		foreach (PropertyInfo propertyInfo in visibleProperties)
		{
			NameAttribute? attribute = propertyInfo.GetCustomAttribute<NameAttribute>();
			string label = attribute?.Name ?? propertyInfo.Name.WordSpaced();
			propertyColumns.Add(new TabPropertyColumn(propertyInfo, label));
		}
		return propertyColumns;
	}
}

public class TabMethodColumn(MethodInfo methodInfo, string? label = null)
{
	[HiddenColumn]
	public MethodInfo MethodInfo => methodInfo;

	public string Label { get; set; } = label ?? methodInfo.GetCustomAttribute<ButtonColumnAttribute>()?.Name ?? methodInfo.Name;
}

public class TabPropertyColumn(PropertyInfo propertyInfo, string label)
{
	[HiddenColumn]
	public PropertyInfo PropertyInfo => propertyInfo;

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
