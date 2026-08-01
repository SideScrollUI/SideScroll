using SideScroll.Serialize.Atlas.Schema;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoEnum(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
			{
				return new TypeRepoEnum(serializer, typeSchema);
			}
			return null;
		}
	}

	// Enums can use any integer type, casting to int throws an InvalidCastException for the others.
	// Int backed enums still write 4 bytes, so existing data keeps loading
	private readonly Type _underlyingType = Enum.GetUnderlyingType(typeSchema.Type!);

	public static bool CanAssign(Type? type)
	{
		return type?.IsEnum == true;
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		switch (Type.GetTypeCode(_underlyingType))
		{
			case TypeCode.Byte: writer.Write((byte)obj); break;
			case TypeCode.SByte: writer.Write((sbyte)obj); break;
			case TypeCode.Int16: writer.Write((short)obj); break;
			case TypeCode.UInt16: writer.Write((ushort)obj); break;
			case TypeCode.Int32: writer.Write((int)obj); break;
			case TypeCode.UInt32: writer.Write((uint)obj); break;
			case TypeCode.Int64: writer.Write((long)obj); break;
			case TypeCode.UInt64: writer.Write((ulong)obj); break;
			default:
				throw new SerializerException("Unhandled enum underlying type",
					new Tag("Type", Type),
					new Tag("UnderlyingType", _underlyingType));
		}
	}

	private object ReadValue()
	{
		return Type.GetTypeCode(_underlyingType) switch
		{
			TypeCode.Byte => Reader!.ReadByte(),
			TypeCode.SByte => Reader!.ReadSByte(),
			TypeCode.Int16 => Reader!.ReadInt16(),
			TypeCode.UInt16 => Reader!.ReadUInt16(),
			TypeCode.Int32 => Reader!.ReadInt32(),
			TypeCode.UInt32 => Reader!.ReadUInt32(),
			TypeCode.Int64 => Reader!.ReadInt64(),
			TypeCode.UInt64 => Reader!.ReadUInt64(),
			_ => throw new SerializerException("Unhandled enum underlying type",
				new Tag("Type", Type),
				new Tag("UnderlyingType", _underlyingType)),
		};
	}

	protected override object? CreateObject(int objectIndex)
	{
		long position = Reader!.BaseStream.Position;
		Reader.BaseStream.Position = ObjectOffsets![objectIndex];

		object? obj = null;
		try
		{
			if (LoadableType!.IsEnum)
			{
				obj = Enum.ToObject(TypeSchema.Type!, ReadValue());
			}
			else
			{
				throw new SerializerException("Unhandled primitive type");
			}
		}
		catch (Exception)
		{
			//log.Add(e);
		}
		Reader.BaseStream.Position = position;

		ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
		return obj;
	}

	public override object LoadObject()
	{
		object obj = Enum.ToObject(TypeSchema.Type!, ReadValue());
		return obj;
	}

	public override void Clone(object source, object dest)
	{
		// assigning won't do anything since it's not a ref
		throw new SerializerException("Not cloneable");
	}
}
