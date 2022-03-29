using Atlas.Core;
using System.Reflection;

namespace Atlas.Extensions;

public static class MemberExtensions
{
	public static bool IsVisible(this FieldInfo fieldInfo)
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

	public static bool IsVisible(this PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType.IsNotPublic)
			return false;

#if !DEBUG
			if (propertyInfo.GetCustomAttribute<DebugOnlyAttribute>() != null)
				return false;
#endif

		return propertyInfo.GetCustomAttribute<HiddenAttribute>() == null && // [Hidden]
			propertyInfo.GetCustomAttribute<HiddenRowAttribute>() == null; // [HiddenRow]
	}
}
