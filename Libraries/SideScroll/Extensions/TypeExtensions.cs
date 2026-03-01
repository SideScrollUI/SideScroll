using SideScroll.Attributes;
using System.Reflection;

namespace SideScroll.Extensions;

/// <summary>
/// Extension methods for Type inspection, reflection, and type checking
/// </summary>
public static class TypeExtensions
{
	/// <summary>
	/// Set of all numeric types including integers and floating-point types
	/// </summary>
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

	/// <summary>
	/// Set of floating-point decimal types (float, double, decimal)
	/// </summary>
	public static HashSet<Type> DecimalTypes { get; set; } =
	[
		typeof(float),
		typeof(double),
		typeof(decimal),
	];

	/// <summary>
	/// Determines whether a type is a numeric type (including nullable numeric types)
	/// </summary>
	public static bool IsNumeric(this Type type)
	{
		type = type.GetNonNullableType();
		return NumericTypes.Contains(type);
	}

	/// <summary>
	/// Determines whether a type is a decimal/floating-point type (including nullable)
	/// </summary>
	public static bool IsDecimal(this Type type)
	{
		type = type.GetNonNullableType();
		return DecimalTypes.Contains(type);
	}

	/// <summary>
	/// Returns the underlying type if the type is Nullable&lt;T&gt;, otherwise returns the type itself
	/// </summary>
	public static Type GetNonNullableType(this Type type)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			return Nullable.GetUnderlyingType(type)!;
		return type;
	}

	/// <summary>
	/// Determines whether a type can be assigned to a generic type definition, checking interfaces and base types
	/// </summary>
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

		return baseType.IsAssignableToGenericType(genericType);
	}

	/// <summary>
	/// Gets the element type for arrays, generic collections, or base types that have element types
	/// </summary>
	public static Type? GetElementTypeForAll(this Type type)
	{
		if (type.HasElementType)
		{
			return type.GetElementType();
		}
		else if (type.GenericTypeArguments.Length > 0)
		{
			return type.GenericTypeArguments[0];
		}
		else if (type.BaseType != null)
		{
			return type.BaseType.GetElementTypeForAll();
		}

		return null;
	}

	/// <summary>
	/// Returns all visible properties (excluding [Hidden], [HiddenColumn], indexed properties, and static properties), ordered by declaration
	/// </summary>
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

	/// <summary>
	/// Returns the first property decorated with the specified attribute type
	/// </summary>
	public static PropertyInfo? GetPropertyWithAttribute<T>(this Type type) where T : Attribute
	{
		return type.GetPropertiesWithAttribute<T>().FirstOrDefault();
	}

	/// <summary>
	/// Returns all properties decorated with the specified attribute type, ordered by declaration
	/// </summary>
	public static List<PropertyInfo> GetPropertiesWithAttribute<T>(this Type type) where T : Attribute
	{
		// Properties are returned in a random order, so sort them by the MetadataToken to get the original order
		return type.GetProperties()
			.Where(p => p.GetCustomAttribute<T>() != null)
			.OrderBy(p => p.Module.Name)
			.ThenBy(p => p.MetadataToken)
			.ToList();
	}

	/// <summary>
	/// Returns all fields decorated with the specified attribute type, ordered by declaration
	/// </summary>
	public static List<FieldInfo> GetFieldsWithAttribute<T>(this Type type) where T : Attribute
	{
		// Fields are returned in a random order, so sort them by the MetadataToken to get the original order
		return type.GetFields()
			.Where(f => f.GetCustomAttribute<T>() != null)
			.OrderBy(f => f.Module.Name)
			.ThenBy(f => f.MetadataToken)
			.ToList();
	}

	/// <summary>
	/// Returns an assembly-qualified name with only the namespace, type, and assembly name (for consistency across versions)
	/// </summary>
	public static string GetAssemblyQualifiedShortName(this Type type)
	{
		string name;
		if (type.IsGenericType)
		{
			var args = type.GetGenericArguments().Select(a => '[' + a.GetAssemblyQualifiedShortName() + ']');
			name = $"{type.GetGenericTypeDefinition().FullName}[{string.Join(", ", args)}]";
		}
		else
		{
			name = type.FullName!;
		}

		if (type.Assembly.GetName().Name is string assemblyName)
		{
			name += ", " + assemblyName;
		}
		return name;
	}
}
