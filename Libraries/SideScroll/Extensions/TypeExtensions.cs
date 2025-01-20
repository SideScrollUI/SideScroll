using SideScroll.Attributes;
using System.Reflection;

namespace SideScroll.Extensions;

public static class TypeExtensions
{
	public static HashSet<Type> NumericTypes { get; set; } =
	[
		typeof(byte),
		typeof(sbyte),

		typeof(short),
		typeof(ushort),

		typeof(int),
		typeof(uint),

		typeof(long),
		typeof(ulong),

		typeof(float),
		typeof(double),
		typeof(decimal),
	];

	public static HashSet<Type> DecimalTypes { get; set; } =
	[
		typeof(float),
		typeof(double),
		typeof(decimal),
	];

	public static bool IsNumeric(this Type type)
	{
		type = GetNonNullableType(type);
		return NumericTypes.Contains(type);
	}

	public static bool IsDecimal(this Type type)
	{
		type = GetNonNullableType(type);
		return DecimalTypes.Contains(type);
	}

	public static Type GetNonNullableType(this Type type)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			return Nullable.GetUnderlyingType(type)!;
		return type;
	}

	public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
	{
		var interfaceTypes = givenType.GetInterfaces();

		foreach (var it in interfaceTypes)
		{
			if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
				return true;
		}

		if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
			return true;

		Type? baseType = givenType.BaseType;
		if (baseType == null)
			return false;

		return IsAssignableToGenericType(baseType, genericType);
	}

	public static Type? GetElementTypeForAll(this Type type)
	{
		if (type.HasElementType)
			return type.GetElementType();
		else if (type.GenericTypeArguments.Length > 0)
			return type.GenericTypeArguments[0];
		else if (type.BaseType != null)
			return GetElementTypeForAll(type.BaseType);

		return null;
	}

	public static List<PropertyInfo> GetVisibleProperties(this Type type)
	{
		return type.GetProperties()
			.Where(p => p.GetCustomAttribute<HiddenAttribute>() == null)
			.Where(p => p.GetCustomAttribute<HiddenColumnAttribute>() == null)
			.Where(p => p.GetIndexParameters().Length == 0)
			.Where(p => !p.GetAccessors(nonPublic: true)[0].IsStatic)
			.OrderBy(p => p.Module.Name)
			.ThenBy(p => p.MetadataToken)
			.ToList();
	}

	public static PropertyInfo? GetPropertyWithAttribute<T>(this Type type) where T : Attribute
	{
		return GetPropertiesWithAttribute<T>(type).FirstOrDefault();
	}

	public static List<PropertyInfo> GetPropertiesWithAttribute<T>(this Type type) where T : Attribute
	{
		// Properties are returned in a random order, so sort them by the MetadataToken to get the original order
		return type.GetProperties()
			.Where(p => p.GetCustomAttribute<T>() != null)
			.OrderBy(p => p.Module.Name)
			.ThenBy(p => p.MetadataToken)
			.ToList();
	}

	public static List<FieldInfo> GetFieldsWithAttribute<T>(this Type type) where T : Attribute
	{
		// Fields are returned in a random order, so sort them by the MetadataToken to get the original order
		return type.GetFields()
			.Where(f => f.GetCustomAttribute<T>() != null)
			.OrderBy(f => f.Module.Name)
			.ThenBy(f => f.MetadataToken)
			.ToList();
	}
}
