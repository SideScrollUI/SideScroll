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
		public string propertyName;
		public int typeIndex = -1;

		public TypeSchema ownerTypeSchema;
		public TypeSchema propertyTypeSchema;
		public PropertyInfo propertyInfo; // can be null

		public Type type; // might be null
		public Type nonNullableType; // might be null

		public bool Serialized { get; set; } // cached copy of IsSerialized

		public bool Loadable;

		public PropertySchema(PropertyInfo propertyInfo)
		{
			propertyName = propertyInfo.Name;
			this.propertyInfo = propertyInfo;
			Serialized = IsSerialized;
			type = propertyInfo.PropertyType;
			nonNullableType = type.GetNonNullableType();
		}

		public PropertySchema(TypeSchema typeSchema, BinaryReader reader)
		{
			this.ownerTypeSchema = typeSchema;
			Load(reader);
			if (typeSchema.type != null)
				propertyInfo = typeSchema.type.GetProperty(propertyName);
			Serialized = IsSerialized;

			if (propertyInfo != null)
			{
				type = propertyInfo.PropertyType;
				nonNullableType = type.GetNonNullableType();
				Loadable = Serialized; // typeIndex >= 0 && // derived types won't have entries for base type
			}
		}

		public override string ToString()
		{
			return propertyName;
		}

		private bool IsSerialized
		{
			get
			{
				if (propertyInfo == null)
					return false;

				Attribute attribute = propertyInfo.GetCustomAttribute(typeof(NonSerializedAttribute));
				if (attribute != null)
					return false;

				attribute = propertyInfo.GetCustomAttribute(typeof(UnserializedAttribute));
				if (attribute != null)
					return false;

				if (propertyInfo.CanRead == false || propertyInfo.CanWrite == false)
					return false;

				if (propertyInfo.GetIndexParameters().Length > 0)
					return false;

				return true;
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(propertyName);
			writer.Write((short)typeIndex);
		}

		public void Load(BinaryReader reader)
		{
			propertyName = reader.ReadString();
			typeIndex = reader.ReadInt16();
		}

		public void Validate(List<TypeSchema> typeSchemas)
		{
			if (typeIndex >= 0)
			{
				TypeSchema typeSchema = typeSchemas[typeIndex];
				if (propertyInfo != null && typeSchema.type != propertyInfo.PropertyType.GetNonNullableType())
					Loadable = false;
			}
		}
	}
}

/*

*/
