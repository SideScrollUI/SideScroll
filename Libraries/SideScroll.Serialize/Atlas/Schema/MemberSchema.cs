using SideScroll.Attributes;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.Schema;

public class MemberSchema(TypeSchema typeSchema, string name)
{
	public TypeSchema OwnerTypeSchema => typeSchema;

	public string Name => name;

	public int TypeIndex { get; set; } = -1;

	public Type? Type { get; protected set; }
	public Type? NonNullableType { get; protected set; }

	public bool IsPrivate { get; protected set; }
	public bool IsPublic { get; protected set; }
	public bool IsReadable { get; set; }

	public void Save(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write((short)TypeIndex);
	}

	public static MemberSchema Load<T>(TypeSchema typeSchema, Serializer serializer, BinaryReader reader) where T : MemberInfo
	{
		string name = reader.ReadString();
		int typeIndex = reader.ReadInt16();

		if (typeSchema.Type == null) return new MemberSchema(typeSchema, name);

		MemberInfo? memberInfo = typeSchema.GetMemberInfo(name);

		if (memberInfo != null && (serializer.EnableFieldToPropertyMapping || memberInfo is T))
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				return new FieldSchema(typeSchema, fieldInfo);
			}

			if (memberInfo is PropertyInfo propertyInfo)
			{
				return new PropertySchema(typeSchema, propertyInfo);
			}
		}
		return new MemberSchema(typeSchema, name);
	}

	protected bool GetIsPrivate(MemberInfo memberInfo)
	{
		if (memberInfo.GetCustomAttribute<PrivateDataAttribute>() != null)
			return true;

		if (Type!.GetCustomAttribute<PrivateDataAttribute>() != null)
			return true;

		return false;
	}

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
