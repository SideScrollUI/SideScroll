using System.Reflection;

namespace SideScroll.Serialize;

/// <summary>
/// Extension methods for serialization and cloning operations
/// </summary>
public static class SerializerExtensions
{
	private const BindingFlags CloneBindingAttr =
		BindingFlags.Public |
		BindingFlags.Instance |
		BindingFlags.FlattenHierarchy;

	/// <summary>
	/// Creates a deep clone of an object by serializing and deserializing it
	/// </summary>
	public static T DeepClone<T>(this T obj, Call? call = null, bool publicOnly = false) where T : class
	{
		call ??= new();
		return SerializerMemory.DeepClone<T>(call, obj, publicOnly);
	}

	/// <summary>
	/// Attempts to create a deep clone of an object, returning null if the operation fails
	/// </summary>
	public static T? TryDeepClone<T>(this T? obj, Call? call = null, bool publicOnly = false) where T : class
	{
		call ??= new();
		return SerializerMemory.TryDeepClone<T>(call, obj, publicOnly);
	}

	/// <summary>
	/// Attempts to create a deep clone of an object, returning null if the operation fails (non-generic version)
	/// </summary>
	public static object? TryDeepClone(this object? obj, Call? call = null, bool publicOnly = false)
	{
		call ??= new();
		return SerializerMemory.TryDeepClone(call, obj, publicOnly);
	}

	/// <summary>
	/// Copies all properties and fields from source to dest using shallow copy (references are copied, not deep cloned)
	/// </summary>
	public static void ShallowClone(this object dest, object source)
	{
		Type inputType = source.GetType();
		Type outputType = dest.GetType();

		if (outputType != inputType && !outputType.IsSubclassOf(inputType))
		{
			throw new ArgumentException($"{outputType} is not a subclass of {inputType}");
		}

		PropertyInfo[] properties = inputType.GetProperties(CloneBindingAttr | BindingFlags.GetProperty | BindingFlags.SetProperty);

		FieldInfo[] fields = inputType.GetFields(CloneBindingAttr);

		foreach (PropertyInfo property in properties)
		{
			try
			{
				property.SetValue(dest, property.GetValue(source, null), null);
			}
			catch (ArgumentException) { } // For Get-only-properties
		}

		foreach (FieldInfo field in fields)
		{
			field.SetValue(dest, field.GetValue(source));
		}
	}
}
