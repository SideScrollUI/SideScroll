using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.Schema;

public class PropertySchema : MemberSchema
{
	public PropertyInfo PropertyInfo { get; }

	public TypeSchema? PropertyTypeSchema { get; set; }

	public bool IsWriteable { get; set; }
	public bool IsRequired { get; set; } // Readonly properties are only serialized if needed by a custom constructor

	public bool ShouldWrite => IsReadable && (IsWriteable || IsRequired);

	public override string ToString() => Name;

	public PropertySchema(TypeSchema typeSchema, PropertyInfo propertyInfo) :
		base(typeSchema, propertyInfo.Name)
	{
		PropertyInfo = propertyInfo;

		Type = PropertyInfo.PropertyType;
		NonNullableType = Type.GetNonNullableType();
		IsReadable = GetIsReadable();
		IsWriteable = IsReadable && PropertyInfo.CanWrite; // typeIndex >= 0
		IsPrivate = GetIsPrivate(PropertyInfo);
		IsPublic = GetIsPublic(PropertyInfo, PropertyInfo.PropertyType);
	}

	private bool GetIsReadable()
	{
		if (PropertyInfo.CanRead == false)
			return false;

		if (PropertyInfo.GetIndexParameters().Length > 0)
			return false;

		// Derived types won't have entries for base type
		if (Type!.GetCustomAttribute<UnserializedAttribute>() != null)
			return false;

		if (PropertyInfo.GetCustomAttribute<NonSerializedAttribute>() != null)
			return false;

		if (PropertyInfo.GetCustomAttribute<UnserializedAttribute>() != null)
			return false;

		return true;
	}

	public void Validate(List<TypeSchema> typeSchemas)
	{
		if (TypeIndex < 0) return;

		TypeSchema typeSchema = typeSchemas[TypeIndex];

		// check if the type has changed
		Type currentType = PropertyInfo.PropertyType.GetNonNullableType();
		if (typeSchema.Type != currentType)
		{
			IsReadable = false;
			IsWriteable = false;
		}
	}
}
