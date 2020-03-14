using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Atlas.Serialize
{
	public class FieldSchema
	{
		public string fieldName;
		public int typeIndex = -1;

		public TypeSchema ownerTypeSchema;
		public TypeSchema typeSchema;
		public FieldInfo fieldInfo; // can be null

		public Type type; // might be null
		public Type nonNullableType; // might be null

		public bool Serialized { get; set; } // cached copy of IsSerialized

		public bool Loadable;

		public FieldSchema(FieldInfo fieldInfo)
		{
			fieldName = fieldInfo.Name;
			this.fieldInfo = fieldInfo;
			Serialized = IsSerialized;
			type = fieldInfo.FieldType;
			nonNullableType = type.GetNonNullableType();
		}

		public FieldSchema(TypeSchema typeSchema, BinaryReader reader)
		{
			this.ownerTypeSchema = typeSchema;
			Load(reader);
			if (typeSchema.type != null)
				fieldInfo = typeSchema.type.GetField(fieldName);
			Serialized = IsSerialized;

			if (fieldInfo != null)
			{
				type = fieldInfo.FieldType;
				nonNullableType = type.GetNonNullableType();
				Loadable = Serialized; // derived types won't have entries for base type
				//Loadable = (typeIndex >= 0 && fieldInfo != null && Serialized);
			}
		}

		public override string ToString() => fieldName;
		
		private bool IsSerialized
		{
			get
			{
				if (fieldInfo == null || fieldInfo.IsLiteral == true || fieldInfo.IsStatic == true)
					return false;

				Attribute attribute = fieldInfo.GetCustomAttribute(typeof(NonSerializedAttribute));
				if (attribute != null)
					return false;

				attribute = fieldInfo.GetCustomAttribute(typeof(UnserializedAttribute));
				if (attribute != null)
					return false;

				return true;
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(fieldName);
			writer.Write((short)typeIndex);
		}

		public void Load(BinaryReader reader)
		{
			fieldName = reader.ReadString();
			typeIndex = reader.ReadInt16();
		}

		public void Validate(List<TypeSchema> typeSchemas)
		{
			if (typeIndex >= 0)
			{
				TypeSchema typeSchema = typeSchemas[typeIndex];
				if (fieldInfo != null && typeSchema.type != fieldInfo.FieldType.GetNonNullableType())
					Loadable = false;
			}
		}
	}
}
