using Atlas.Core;
using System;
using System.Reflection;

namespace Atlas.Serialize;

public static class SerializerExtensions
{
	private const BindingFlags CloneBindingAttr =
		BindingFlags.Public |
		BindingFlags.NonPublic |
		BindingFlags.Instance |
		BindingFlags.FlattenHierarchy;

	public static T DeepClone<T>(this T obj, Call call = null, bool publicOnly = false)
	{
		call ??= new Call();
		return SerializerMemory.DeepClone<T>(call, obj, publicOnly);
	}

	public static object DeepClone(this object obj, Call call = null, bool publicOnly = false)
	{
		call ??= new Call();
		return SerializerMemory.DeepClone(call, obj, publicOnly);
	}

	public static void CloneParentClass(this object dest, object source)
	{
		Type inputType = source.GetType();
		Type outputType = dest.GetType();

		if (outputType != inputType && !outputType.IsSubclassOf(inputType))
			throw new ArgumentException($"{outputType} is not a subclass of {inputType}");

		PropertyInfo[] properties = inputType.GetProperties(CloneBindingAttr);

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
