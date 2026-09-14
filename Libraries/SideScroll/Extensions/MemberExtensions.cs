using SideScroll.Attributes;
using System.Reflection;

namespace SideScroll.Extensions;

/// <summary>
/// Extension methods for MemberInfo, FieldInfo, and PropertyInfo to determine visibility in data displays
/// </summary>
public static class MemberExtensions
{
	/// <summary>
	/// Returns the name a method has in source: the declared name for an ordinary method, the local function's own name for a compiler-generated local function, and the enclosing method's name for a lambda
	/// </summary>
	/// <remarks>
	/// The compiler names a lambda <c>&lt;Outer&gt;b__0_1</c> and a local function <c>&lt;Outer&gt;g__Name|0_1</c>,
	/// which is what a task built from either showed as its label
	/// </remarks>
	public static string GetSourceName(this MethodInfo methodInfo)
	{
		string name = methodInfo.Name;
		if (!name.StartsWith('<'))
			return name;

		// A local function's name sits between g__ and |
		int localStart = name.IndexOf(">g__", StringComparison.Ordinal);
		if (localStart >= 0)
		{
			localStart += 4;
			int localEnd = name.IndexOf('|', localStart);
			return localEnd > localStart ? name[localStart..localEnd] : name[localStart..];
		}

		// A lambda carries only the enclosing method, which top-level statements nest as <<Main>$>
		int outerStart = name.LastIndexOf('<') + 1;
		int outerEnd = name.IndexOf('>', outerStart);
		return outerEnd > outerStart ? name[outerStart..outerEnd] : name;
	}

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
