using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.Reflection;

namespace SideScroll.Tabs.Settings;

public enum SelectionType
{
	None,
	User,
	Auto,
}

[PublicData]
public class TabDataSettings
{
	public HashSet<SelectedRow> SelectedRows { get; set; } = []; // needs to be nullable or we need another initialized value
	public SelectionType SelectionType { get; set; } = SelectionType.None;
	public List<string> ColumnNameOrder { get; set; } = []; // Order to show the columns in, users can drag columns around to reorder these

	// Not currently supported by DataGrid
	// public string? SortColumnName { get; set; } // Currently sorted column
	// public ListSortDirection SortDirection { get; set; }

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
		var methodInfos = type.GetMethods()
			.OrderBy(m => m.Module.Name)
			.ThenBy(m => m.MetadataToken);

		var callableMethods = new List<MethodColumn>();
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

	private static readonly Dictionary<Type, List<PropertyInfo>> _visiblePropertiesCache = [];

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
		[HiddenColumn]
		public MethodInfo MethodInfo => methodInfo;

		public string Label { get; set; } = label ?? methodInfo.GetCustomAttribute<ButtonColumnAttribute>()?.Name ?? methodInfo.Name;
	}

	public class PropertyColumn(PropertyInfo propertyInfo, string label)
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

	private List<PropertyInfo> GetPropertyColumns(Type elementType)
	{
		List<PropertyInfo> visibleProperties = GetVisibleProperties(elementType);
		if (ColumnNameOrder?.Count > 0)
		{
			var propertyNames = visibleProperties.ToDictionary(propertyInfo => propertyInfo.Name);

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
		foreach (PropertyInfo propertyInfo in visibleProperties)
		{
			NameAttribute? attribute = propertyInfo.GetCustomAttribute<NameAttribute>();
			string label = attribute?.Name ?? propertyInfo.Name.WordSpaced();
			propertyColumns.Add(new PropertyColumn(propertyInfo, label));
		}
		return propertyColumns;
	}
}
