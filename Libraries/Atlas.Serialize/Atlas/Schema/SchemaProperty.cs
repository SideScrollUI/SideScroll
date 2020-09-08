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

		public bool Serialized { get; set; } // cached copy of IsSerialized

		public bool Loadable;

		public PropertySchema(PropertyInfo propertyInfo)
		{
			PropertyName = propertyInfo.Name;
			PropertyInfo = propertyInfo;
			Type = propertyInfo.PropertyType;
			Serialized = IsSerialized;
			NonNullableType = Type.GetNonNullableType();
		}

		public PropertySchema(TypeSchema typeSchema, BinaryReader reader)
		{
			OwnerTypeSchema = typeSchema;
			Load(reader);
			try
			{
				if (typeSchema.Type != null)
					PropertyInfo = typeSchema.Type.GetProperty(PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			}
			catch (Exception)
			{
			}
			Serialized = IsSerialized;

			if (PropertyInfo != null)
			{
				Type = PropertyInfo.PropertyType;
				NonNullableType = Type.GetNonNullableType();
				Loadable = Serialized; // typeIndex >= 0 && // derived types won't have entries for base type
			}
		}

		public override string ToString() => PropertyName;

		private bool IsSerialized
		{
			get
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
			if (TypeIndex >= 0)
			{
				TypeSchema typeSchema = typeSchemas[TypeIndex];
				if (PropertyInfo != null)
				{
					// check if the type has changed
					Type currentType = PropertyInfo.PropertyType.GetNonNullableType();
					if (typeSchema.Type != currentType)
						Loadable = false;
				}
			}
		}
	}
}
