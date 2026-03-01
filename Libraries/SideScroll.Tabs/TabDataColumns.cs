using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.Reflection;

namespace SideScroll.Tabs;

/// <summary>
/// Manages the columns displayed in tab data grids, including property and method columns
/// </summary>
public class TabDataColumns(List<string>? columnNameOrder = null)
{
	/// <summary>
	/// Order to show the columns in. Users can drag columns around to reorder these
	/// </summary>
	public List<string> ColumnNameOrder { get; set; } = columnNameOrder ?? [];

	private static readonly Dictionary<Type, List<PropertyInfo>> VisiblePropertiesCache = [];

	/// <summary>
	/// Gets the method columns for a type based on ButtonColumnAttribute annotations
	/// </summary>
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

	/// <summary>
	/// Gets the visible properties for a type, using caching for performance
	/// </summary>
	public static List<PropertyInfo> GetVisibleProperties(Type type)
	{
		lock (VisiblePropertiesCache)
		{
			if (VisiblePropertiesCache.TryGetValue(type, out List<PropertyInfo>? list))
				return list;

			list = type.GetVisibleProperties();
			VisiblePropertiesCache.Add(type, list);
			return list;
		}
	}

	/// <summary>
	/// Gets the visible properties for the element type of a list
	/// </summary>
	public static List<PropertyInfo> GetVisibleElementProperties(IList list)
	{
		Type listType = list.GetType();
		Type elementType = listType.GetGenericArguments()[0]; // Dictionaries?
		return GetVisibleProperties(elementType);
	}

	private List<PropertyInfo> GetOrderedPropertyColumns(Type elementType)
	{
		List<PropertyInfo> visibleProperties = GetVisibleProperties(elementType);
		if (ColumnNameOrder.Count > 0)
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

	/// <summary>
	/// Gets the property columns for a type, ordered according to the ColumnNameOrder
	/// </summary>
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

/// <summary>
/// Represents a column backed by a method with a ButtonColumnAttribute
/// </summary>
public class TabMethodColumn(MethodInfo methodInfo, string? label = null)
{
	/// <summary>
	/// Gets the method info for this column
	/// </summary>
	[HiddenColumn]
	public MethodInfo MethodInfo => methodInfo;

	/// <summary>
	/// Gets or sets the display label for this column
	/// </summary>
	public string Label { get; set; } = label ?? methodInfo.GetCustomAttribute<ButtonColumnAttribute>()?.Name ?? methodInfo.Name;
}

/// <summary>
/// Represents a column backed by a property
/// </summary>
public class TabPropertyColumn(PropertyInfo propertyInfo, string label)
{
	/// <summary>
	/// Gets the property info for this column
	/// </summary>
	[HiddenColumn]
	public PropertyInfo PropertyInfo => propertyInfo;

	/// <summary>
	/// Gets or sets the display label for this column
	/// </summary>
	public string Label { get; set; } = label;

	public override string ToString() => Label;

	/// <summary>
	/// Determines if this column should be styled based on StyleValueAttribute or type
	/// </summary>
	public bool IsStyled()
	{
		return PropertyInfo.IsDefined(typeof(StyleValueAttribute)) ||
			typeof(DictionaryEntry).IsAssignableFrom(PropertyInfo.DeclaringType);
	}

	/// <summary>
	/// Determines if this column should be visible by checking HideAttribute and HideColumnAttribute, then evaluating visibility for each item in the list
	/// </summary>
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
