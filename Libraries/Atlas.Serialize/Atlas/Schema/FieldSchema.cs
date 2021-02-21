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
		public string FieldName;
		public int TypeIndex = -1;

		public TypeSchema OwnerTypeSchema;
		public TypeSchema TypeSchema;
		public FieldInfo FieldInfo; // can be null

		public Type Type; // might be null
		public Type NonNullableType; // might be null

		public bool IsSerialized;
		public bool IsLoadable;
		public bool IsPrivate;

		public override string ToString() => FieldName;

		public FieldSchema(FieldInfo fieldInfo)
		{
			FieldName = fieldInfo.Name;
			FieldInfo = fieldInfo;

			Initialize();
		}

		public FieldSchema(TypeSchema typeSchema, BinaryReader reader)
		{
			OwnerTypeSchema = typeSchema;
			Load(reader);

			if (typeSchema.Type != null)
				FieldInfo = typeSchema.Type.GetField(FieldName);

			Initialize();
		}

		private void Initialize()
		{
			if (FieldInfo != null)
			{
				Type = FieldInfo.FieldType;
				NonNullableType = Type.GetNonNullableType();
				IsSerialized = GetIsSerialized();
				IsLoadable = IsSerialized; // derived types won't have entries for base type
				IsPrivate = (FieldInfo.GetCustomAttribute<PrivateDataAttribute>() != null);
			}
		}

		private bool GetIsSerialized()
		{
			if (FieldInfo.IsLiteral == true || FieldInfo.IsStatic == true)
				return false;

			Attribute attribute = Type.GetCustomAttribute<UnserializedAttribute>();
			if (attribute != null)
				return false;

			attribute = FieldInfo.GetCustomAttribute<NonSerializedAttribute>();
			if (attribute != null)
				return false;

			attribute = FieldInfo.GetCustomAttribute<UnserializedAttribute>();
			if (attribute != null)
				return false;

			return true;
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(FieldName);
			writer.Write((short)TypeIndex);
		}

		public void Load(BinaryReader reader)
		{
			FieldName = reader.ReadString();
			TypeIndex = reader.ReadInt16();
		}

		public void Validate(List<TypeSchema> typeSchemas)
		{
			if (TypeIndex < 0)
				return;
			
			TypeSchema typeSchema = typeSchemas[TypeIndex];
			if (FieldInfo != null && typeSchema.Type != FieldInfo.FieldType.GetNonNullableType())
				IsLoadable = false;
		}
	}
}
