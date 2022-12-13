using Atlas.Core;
using Atlas.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Atlas.Serialize;

public class PropertySchema
{
	public string PropertyName;
	public int TypeIndex = -1;

	public TypeSchema OwnerTypeSchema;
	public TypeSchema? PropertyTypeSchema;
	public PropertyInfo? PropertyInfo;

	public Type? Type;
	public Type? NonNullableType;

	public bool IsSerialized;
	public bool IsLoadable;
	public bool IsPrivate;
	public bool IsPublic;

	public override string ToString() => PropertyName;

	public PropertySchema(TypeSchema typeSchema, PropertyInfo propertyInfo)
	{
		OwnerTypeSchema = typeSchema;
		PropertyInfo = propertyInfo;
		PropertyName = propertyInfo.Name;

		Initialize();
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

		Initialize();
	}

	private void Initialize()
	{
		if (PropertyInfo != null)
		{
			Type = PropertyInfo.PropertyType;
			NonNullableType = Type.GetNonNullableType();
			IsSerialized = GetIsSerialized();
			IsLoadable = IsSerialized; // typeIndex >= 0 && // derived types won't have entries for base type
			IsPrivate = GetIsPrivate();
			IsPublic = GetIsPublic();
		}
	}

	private bool GetIsSerialized()
	{
		Attribute? attribute = Type!.GetCustomAttribute<UnserializedAttribute>();
		if (attribute != null)
			return false;

		attribute = PropertyInfo!.GetCustomAttribute<NonSerializedAttribute>();
		if (attribute != null)
			return false;

		attribute = PropertyInfo!.GetCustomAttribute<UnserializedAttribute>();
		if (attribute != null)
			return false;

		if (PropertyInfo!.CanRead == false || PropertyInfo.CanWrite == false)
			return false;

		if (PropertyInfo.GetIndexParameters().Length > 0)
			return false;

		return true;
	}

	private bool GetIsPrivate()
	{
		if (PropertyInfo!.GetCustomAttribute<PrivateDataAttribute>() != null)
			return true;

		if (Type!.GetCustomAttribute<PrivateDataAttribute>() != null)
			return true;

		return false;
	}

	private bool GetIsPublic()
	{
		if (PropertyInfo!.GetCustomAttribute<PublicDataAttribute>() != null)
			return true;

		if (PropertyInfo!.GetCustomAttribute<ProtectedDataAttribute>() != null)
			return true;

		if (PropertyInfo!.PropertyType.GetCustomAttribute<PublicDataAttribute>() != null)
			return true;

		if (PropertyInfo!.PropertyType.GetCustomAttribute<ProtectedDataAttribute>() != null)
			return true;

		if (OwnerTypeSchema.IsProtected)
			return false;

		return true;
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(PropertyName!);
		writer.Write((short)TypeIndex);
	}

	[MemberNotNull(nameof(PropertyName), nameof(TypeIndex))]
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
