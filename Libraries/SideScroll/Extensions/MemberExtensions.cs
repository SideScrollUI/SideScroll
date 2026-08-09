using SideScroll.Attributes;
using System.Reflection;

namespace SideScroll.Extensions;

/// <summary>
/// Extension methods for MemberInfo, FieldInfo, and PropertyInfo to determine visibility in data displays
/// </summary>
public static class MemberExtensions
{
	/// <summary>
	/// Determines whether a field should be visible as a row in data displays (excludes constants, debug-only fields, and [Hidden]/[HiddenRow] fields)
	/// </summary>
	public static bool IsRowVisible(this FieldInfo fieldInfo)
	{
		// IsLiteral alone, a const is never also readonly so !IsInitOnly was always true
		if (fieldInfo.IsLiteral)
			return false;

#if !DEBUG
			if (fieldInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return fieldInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			fieldInfo.GetCustomAttribute<HiddenRowAttribute>() == null; // [HiddenRow]
	}

	/// <summary>
	/// Determines whether a property should be visible as a row in data displays (excludes non-public types, debug-only properties, and [Hidden]/[HiddenRow] properties)
	/// </summary>
	public static bool IsRowVisible(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType!.IsNotPublic || propertyInfo.GetMethod?.IsPublic != true)
			return false;

#if !DEBUG
			if (propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return propertyInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			propertyInfo.GetCustomAttribute<HiddenRowAttribute>() == null; // [HiddenRow]
	}

	/// <summary>
	/// Determines whether a property should be visible as a column in DataGrids (excludes non-public types, debug-only properties, and [Hidden]/[HiddenColumn] properties)
	/// </summary>
	public static bool IsColumnVisible(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType!.IsNotPublic || propertyInfo.GetMethod?.IsPublic != true)
			return false;

#if !DEBUG
			if (propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return propertyInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			propertyInfo.GetCustomAttribute<HiddenColumnAttribute>() == null; // [HiddenRow]
	}
}
