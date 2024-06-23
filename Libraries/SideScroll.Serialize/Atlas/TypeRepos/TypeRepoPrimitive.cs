using SideScroll.Serialize.Atlas.Schema;
using System.Diagnostics;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoPrimitive(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type!))
				return new TypeRepoPrimitive(serializer, typeSchema);
			return null;
		}
	}

	public static bool CanAssign(Type type)
	{
		return type.IsPrimitive;
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		if (obj is uint u)
			writer.Write(u);
		else if (obj is int i)
			writer.Write(i);
		else if (obj is long l)
			writer.Write(l);
		else if (obj is double d)
			writer.Write(d);
		else if (obj is float f)
			writer.Write((double)f); // there's no ReadFloat() routine
		else if (obj is bool b)
			writer.Write(b); // 1 byte
		else if (obj is char c)
			writer.Write(c); // there's no ReadFloat() routine
		else if (obj is byte bt)
			writer.Write(bt);
		else
			Debug.Assert(true);
	}

	protected override object? CreateObject(int objectIndex)
	{
		return null;
	}

	public override object LoadObject()
	{
		object obj;
		if (Type == typeof(uint))
			obj = Reader!.ReadUInt32();
		else if (Type == typeof(int))
			obj = Reader!.ReadInt32();
		else if (Type == typeof(long))
			obj = Reader!.ReadInt64();
		else if (Type == typeof(double))
			obj = Reader!.ReadDouble();
		else if (Type == typeof(float))
			obj = (float)Reader!.ReadDouble();
		else if (Type == typeof(bool))
			obj = Reader!.ReadBoolean();
		else if (Type == typeof(char))
			obj = Reader!.ReadChar();
		else if (Type == typeof(byte))
			obj = Reader!.ReadByte();
		else
			throw new Exception($"Unhandled primitive type: {Type}");

		return obj;
	}

	public override void Clone(object source, object dest)
	{
		// assigning won't do anything since it's not a ref
		throw new Exception("Not cloneable");
	}
}
