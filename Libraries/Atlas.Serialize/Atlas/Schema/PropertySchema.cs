using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Atlas.Serialize
{
	public class PropertySchema
	{
		public string PropertyName;
		public int TypeIndex = -1;

		public TypeSchema OwnerTypeSchema;
		public TypeSchema PropertyTypeSchema;
		public PropertyInfo PropertyInfo; // can be null

		public Type Type; // might be null
		public Type NonNullableType; // might be null

		public bool IsSerialized;
		public bool IsLoadable;

		public override string ToString() => PropertyName;

		public PropertySchema(PropertyInfo propertyInfo)
		{
			PropertyName = propertyInfo.Name;
			PropertyInfo = propertyInfo;
			Type = propertyInfo.PropertyType;
			NonNullableType = Type.GetNonNullableType();
			IsSerialized = GetIsSerialized();
		}

		public PropertySchema(TypeSchema typeSchema, BinaryReader reader)
		{
			OwnerTypeSchema = typeSchema;
			Load(reader);
			try
			{
				if (typeSchema.Type != null)
				{
					PropertyInfo = typeSchema.Type.GetProperty(PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
				}
			}
			catch (Exception)
			{
			}
			IsSerialized = GetIsSerialized();

			if (PropertyInfo != null)
			{
				Type = PropertyInfo.PropertyType;
				NonNullableType = Type.GetNonNullableType();
				IsLoadable = IsSerialized; // typeIndex >= 0 && // derived types won't have entries for base type
			}
		}

		private bool GetIsSerialized()
		{
			if (PropertyInfo == null)
				return false;

			Attribute attribute = Type?.GetCustomAttribute<UnserializedAttribute>();
			if (attribute != null)
				return false;

			attribute = PropertyInfo.GetCustomAttribute<NonSerializedAttribute>();
			if (attribute != null)
				return false;

			attribute = PropertyInfo.GetCustomAttribute<UnserializedAttribute>();
			if (attribute != null)
				return false;

			if (PropertyInfo.CanRead == false || PropertyInfo.CanWrite == false)
				return false;

			if (PropertyInfo.GetIndexParameters().Length > 0)
				return false;

			return true;
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(PropertyName);
			writer.Write((short)TypeIndex);
		}

		public void Load(BinaryReader reader)
		{
			PropertyName = reader.ReadString();
			TypeIndex = reader.ReadInt16();
		}

		public void Validate(List<TypeSchema> typeSchemas)
		{
			if (TypeIndex < 0)
				return;
			
			TypeSchema typeSchema = typeSchemas[TypeIndex];
			if (PropertyInfo != null)
			{
				// check if the type has changed
				Type currentType = PropertyInfo.PropertyType.GetNonNullableType();
				if (typeSchema.Type != currentType)
					IsLoadable = false;
			}
		}
	}
}
