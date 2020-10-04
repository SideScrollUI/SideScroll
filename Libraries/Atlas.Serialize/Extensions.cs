using Atlas.Core;
using System;
using System.Reflection;

namespace Atlas.Serialize
{
	public static class SerializerExtensions
	{
		public static T DeepClone<T>(this T obj, Call call = null, bool publicOnly = false)
		{
			call = call ?? new Call();
			return SerializerMemory.DeepClone<T>(call, obj, publicOnly);
		}

		public static object DeepClone(this object obj, Call call = null, bool publicOnly = false)
		{
			call = call ?? new Call();
			return SerializerMemory.DeepClone(call, obj, publicOnly);
		}

		public static void CloneParentClass(this object dest, object source)
		{
			Type inputType = source.GetType();
			Type outputType = dest.GetType();
			if (!outputType.Equals(inputType) && !outputType.IsSubclassOf(inputType))
				throw new ArgumentException(string.Format("{0} is not a sublcass of {1}", outputType, inputType));

			PropertyInfo[] properties = inputType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			FieldInfo[] fields = inputType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
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
}
