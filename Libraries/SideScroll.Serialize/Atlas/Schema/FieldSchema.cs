using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.Schema;

public class FieldSchema : MemberSchema
{
	public FieldInfo FieldInfo { get; }

	public TypeSchema? FieldTypeSchema { get; set; }

	public override string ToString() => Name;

	public FieldSchema(TypeSchema typeSchema, FieldInfo fieldInfo) :
		base(typeSchema, fieldInfo.Name)
	{
		FieldInfo = fieldInfo;

		Type = FieldInfo.FieldType;
		NonNullableType = Type.GetNonNullableType();
		IsReadable = GetIsReadable();
		IsPrivate = GetIsPrivate(FieldInfo);
		IsPublic = GetIsPublic(FieldInfo, FieldInfo.FieldType);
	}

	private bool GetIsReadable()
	{
		if (FieldInfo.IsLiteral || FieldInfo.IsStatic)
			return false;

		// Derived types won't have entries for base type
		if (Type!.GetCustomAttribute<UnserializedAttribute>() != null)
			return false;

		if (FieldInfo.GetCustomAttribute<NonSerializedAttribute>() != null)
			return false;

		if (FieldInfo.GetCustomAttribute<UnserializedAttribute>() != null)
			return false;

		return true;
	}

	public void Validate(List<TypeSchema> typeSchemas)
	{
		if (TypeIndex < 0)
			return;

		TypeSchema typeSchema = typeSchemas[TypeIndex];
		if (typeSchema.Type != FieldInfo.FieldType.GetNonNullableType())
		{
			IsReadable = false;
		}
	}
}
