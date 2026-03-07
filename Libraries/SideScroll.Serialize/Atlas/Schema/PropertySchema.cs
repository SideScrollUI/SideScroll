using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.Schema;

/// <summary>
/// Represents schema information for a property member
/// </summary>
public class PropertySchema : MemberSchema
{
	/// <summary>
	/// Gets the property information from reflection
	/// </summary>
	public PropertyInfo PropertyInfo { get; }

	/// <summary>
	/// Gets or sets the type schema for the property's type
	/// </summary>
	public TypeSchema? PropertyTypeSchema { get; set; }

	/// <summary>
	/// Gets or sets whether the property can be written during deserialization
	/// </summary>
	public bool IsWriteable { get; set; }
	
	/// <summary>
	/// Gets or sets whether the property is required by a custom constructor
	/// Readonly properties are only serialized if needed by a custom constructor
	/// </summary>
	public bool IsRequired { get; set; }

	/// <summary>
	/// Gets whether the property should be serialized (readable and either writeable or required)
	/// </summary>
	public bool ShouldWrite => IsReadable && (IsWriteable || IsRequired);

	public override string ToString() => Name;

	/// <summary>
	/// Initializes a new instance of the PropertySchema class
	/// </summary>
	public PropertySchema(TypeSchema typeSchema, PropertyInfo propertyInfo, int typeIndex = -1) :
		base(typeSchema, propertyInfo.Name, typeIndex)
	{
		PropertyInfo = propertyInfo;

		Type = PropertyInfo.PropertyType;
		NonNullableType = Type.GetNonNullableType();
		IsReadable = GetIsReadable();
		IsWriteable = IsReadable && PropertyInfo.CanWrite; // typeIndex >= 0
		IsPrivate = GetIsPrivate(PropertyInfo);
		IsPublic = GetIsPublic(PropertyInfo, PropertyInfo.PropertyType);
	}

	/// <summary>
	/// Determines whether the property can be serialized
	/// </summary>
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

	/// <summary>
	/// Validates the property schema against the list of type schemas
	/// </summary>
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
