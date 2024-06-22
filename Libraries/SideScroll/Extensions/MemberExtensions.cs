using SideScroll;
using System.Reflection;

namespace SideScroll.Extensions;

public static class MemberExtensions
{
	public static bool IsRowVisible(this FieldInfo fieldInfo)
	{
		if (fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
			return false;

#if !DEBUG
			if (fieldInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return fieldInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			fieldInfo.GetCustomAttribute<HiddenRowAttribute>() == null; // [HiddenRow]
	}

	public static bool IsRowVisible(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType!.IsNotPublic)
			return false;

#if !DEBUG
			if (propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return propertyInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			propertyInfo.GetCustomAttribute<HiddenRowAttribute>() == null; // [HiddenRow]
	}

	public static bool IsColumnVisible(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType!.IsNotPublic)
			return false;

#if !DEBUG
			if (propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return propertyInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			propertyInfo.GetCustomAttribute<HiddenColumnAttribute>() == null; // [HiddenRow]
	}
}
