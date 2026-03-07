using SideScroll.Attributes;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.Schema;

/// <summary>
/// Base class for member schema information (fields and properties)
/// </summary>
public class MemberSchema(TypeSchema typeSchema, string name, int typeIndex = -1)
{
	/// <summary>
	/// Gets the type schema that owns this member
	/// </summary>
	public TypeSchema OwnerTypeSchema => typeSchema;

	/// <summary>
	/// Gets the member name
	/// </summary>
	public string Name => name;

	/// <summary>
	/// Gets or sets the type index for this member's type
	/// </summary>
	public int TypeIndex { get; set; } = typeIndex;

	/// <summary>
	/// Gets or sets the member's type
	/// </summary>
	public Type? Type { get; protected set; }
	
	/// <summary>
	/// Gets or sets the non-nullable version of the member's type
	/// </summary>
	public Type? NonNullableType { get; protected set; }
	
	/// <summary>
	/// Gets whether the member type is nullable
	/// </summary>
	public bool IsNullable => Type != NonNullableType;

	/// <summary>
	/// Gets or sets whether the member is marked as private data
	/// </summary>
	public bool IsPrivate { get; protected set; }
	
	/// <summary>
	/// Gets or sets whether the member is marked as public data
	/// Only public data will be exported if the Serializer is set to PublicOnly
	/// </summary>
	public bool IsPublic { get; protected set; }
	
	/// <summary>
	/// Gets or sets whether the member can be read during serialization
	/// </summary>
	public bool IsReadable { get; set; }

	public override string ToString() => Name;

	/// <summary>
	/// Saves the member schema to a binary writer
	/// </summary>
	public void Save(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write((short)TypeIndex);
	}

	/// <summary>
	/// Loads a member schema from a binary reader
	/// </summary>
	public static MemberSchema Load<T>(TypeSchema typeSchema, Serializer serializer, BinaryReader reader) where T : MemberInfo
	{
		string name = reader.ReadString();
		int typeIndex = reader.ReadInt16();

		if (typeSchema.Type == null) return new MemberSchema(typeSchema, name, typeIndex);

		MemberInfo? memberInfo = typeSchema.GetMemberInfo(name);

		if (memberInfo != null && (serializer.EnableFieldToPropertyMapping || memberInfo is T))
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				return new FieldSchema(typeSchema, fieldInfo, typeIndex);
			}

			if (memberInfo is PropertyInfo propertyInfo)
			{
				return new PropertySchema(typeSchema, propertyInfo, typeIndex);
			}
		}
		return new MemberSchema(typeSchema, name, typeIndex);
	}

	/// <summary>
	/// Determines whether the member is marked as private data
	/// </summary>
	protected bool GetIsPrivate(MemberInfo memberInfo)
	{
		if (memberInfo.GetCustomAttribute<PrivateDataAttribute>() != null)
			return true;

		if (Type!.GetCustomAttribute<PrivateDataAttribute>() != null)
			return true;

		return false;
	}

	/// <summary>
	/// Determines whether the member is marked as public data
	/// </summary>
	protected bool GetIsPublic(MemberInfo memberInfo, Type memberType)
	{
		if (memberInfo.GetCustomAttribute<PublicDataAttribute>() != null)
			return true;

		if (memberInfo.GetCustomAttribute<ProtectedDataAttribute>() != null)
			return true;

		if (memberType.GetCustomAttribute<PublicDataAttribute>() != null)
			return true;

		if (memberType.GetCustomAttribute<ProtectedDataAttribute>() != null)
			return true;

		if (OwnerTypeSchema.IsProtected)
			return false;

		return true;
	}
}
