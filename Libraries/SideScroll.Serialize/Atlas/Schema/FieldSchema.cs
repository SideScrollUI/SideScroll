using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.Schema;

/// <summary>
/// Represents schema information for a field member
/// </summary>
public class FieldSchema : MemberSchema
{
	/// <summary>
	/// Gets the field information from reflection
	/// </summary>
	public FieldInfo FieldInfo { get; }

	/// <summary>
	/// Gets or sets the type schema for the field's type
	/// </summary>
	public TypeSchema? FieldTypeSchema { get; set; }

	public override string ToString() => Name;

	/// <summary>
	/// Initializes a new instance of the FieldSchema class
	/// </summary>
	public FieldSchema(TypeSchema typeSchema, FieldInfo fieldInfo, int typeIndex = -1) :
		base(typeSchema, fieldInfo.Name, typeIndex)
	{
		FieldInfo = fieldInfo;

		Type = FieldInfo.FieldType;
		NonNullableType = Type.GetNonNullableType();

		IsReadable = GetIsReadable();
		IsPrivate = GetIsPrivate(FieldInfo);
		IsPublic = GetIsPublic(FieldInfo, FieldInfo.FieldType);
	}

	/// <summary>
	/// Determines whether the field can be serialized
	/// </summary>
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

	/// <summary>
	/// Validates the field schema against the list of type schemas
	/// </summary>
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
