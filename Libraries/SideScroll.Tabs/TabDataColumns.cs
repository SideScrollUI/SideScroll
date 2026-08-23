using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.Collections.ObjectModel;
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

	// Read only, this is handed to every caller asking about a type. Returning the list itself let
	// one of them sort, clear, or append through it and change the columns for every later grid
	private static readonly Dictionary<Type, ReadOnlyCollection<PropertyInfo>> VisiblePropertiesCache = [];

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

			// The grid invokes these with no arguments, so one needing any, or needing a type
			// argument bound first, threw when its button was pressed rather than being left out
			if (methodInfo.IsGenericMethodDefinition || methodInfo.GetParameters().Length > 0)
				continue;

			methodColumns.Add(new TabMethodColumn(methodInfo, attribute.Name ?? methodInfo.Name));
		}
		return methodColumns;
	}

	/// <summary>
	/// Gets the visible properties for a type, using caching for performance
	/// </summary>
	public static IReadOnlyList<PropertyInfo> GetVisibleProperties(Type type)
	{
		lock (VisiblePropertiesCache)
		{
			if (VisiblePropertiesCache.TryGetValue(type, out ReadOnlyCollection<PropertyInfo>? list))
				return list;

			// Reflection returns both declarations for a property a subclass redeclares with a
			// different type, which showed as two columns with the same name, one of them bound to
			// the declaration the other hides. Merging here rather than at the point of use also
			// covers the filter and the column count heuristic, and is cached with the rest
			List<PropertyInfo> propertyInfos = type.GetVisibleProperties();
			List<PropertyInfo> merged = [];
			var merger = new MemberNameMerger<PropertyInfo>(merged, propertyInfos.Count);
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				merger.AddOrReplace(propertyInfo.Name, propertyInfo);
			}

			list = merged.AsReadOnly();
			VisiblePropertiesCache.Add(type, list);
			return list;
		}
	}

	/// <summary>
	/// Gets the visible properties for the element type of a list
	/// </summary>
	public static IReadOnlyList<PropertyInfo> GetVisibleElementProperties(IList list)
	{
		Type listType = list.GetType();
		Type? elementType = listType.GetElementTypeForAll();
		if (elementType == null) return [];
		return GetVisibleProperties(elementType);
	}

	private IReadOnlyList<PropertyInfo> GetOrderedPropertyColumns(Type elementType)
	{
		// Names are unique, GetVisibleProperties() merges a redeclaration reflection returns twice.
		// ToDictionary() threw here for the duplicate name before it did
		IReadOnlyList<PropertyInfo> visibleProperties = GetVisibleProperties(elementType);
		if (ColumnNameOrder.Count > 0)
		{
			Dictionary<string, PropertyInfo> propertyNames = visibleProperties.ToDictionary(propertyInfo => propertyInfo.Name);

			// Add all previously seen property infos
			List<PropertyInfo> orderedPropertyInfos = [];
			foreach (string columnName in ColumnNameOrder)
			{
				if (propertyNames.Remove(columnName, out PropertyInfo? propertyInfo))
				{
					orderedPropertyInfos.Add(propertyInfo);
				}
			}
			// Add remaining properties in their original order
			foreach (PropertyInfo propertyInfo in visibleProperties)
			{
				if (propertyNames.ContainsKey(propertyInfo.Name))
				{
					orderedPropertyInfos.Add(propertyInfo);
				}
			}
			return orderedPropertyInfos;
		}
		return visibleProperties;
	}

	/// <summary>
	/// Gets the property columns for a type, ordered according to the ColumnNameOrder
	/// </summary>
	public List<TabPropertyColumn> GetPropertyColumns(Type elementType)
	{
		IReadOnlyList<PropertyInfo> visibleProperties = GetOrderedPropertyColumns(elementType);

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

	/// <summary>Returns the column's <see cref="Label"/>.</summary>
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
			catch
			{
			}
		}
		return false;
	}
}
