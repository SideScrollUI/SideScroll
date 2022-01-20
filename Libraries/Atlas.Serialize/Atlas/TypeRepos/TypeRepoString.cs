using System;
using System.IO;

namespace Atlas.Serialize;

public class TypeRepoString : TypeRepo, IDisposable
{
	public class Creator : IRepoCreator
	{
		public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (typeSchema.Type == typeof(string))
				return new TypeRepoString(serializer, typeSchema);
			return null;
		}
	}

	public TypeRepoString(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		writer.Write((string)obj);
	}

	protected override object CreateObject(int objectIndex)
	{
		long position = Reader.BaseStream.Position;
		Reader.BaseStream.Position = ObjectOffsets[objectIndex];

		object obj = Reader.ReadString();
		Reader.BaseStream.Position = position;

		ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
		return obj;
	}

	public override void Clone(object source, object dest)
	{
		// assigning won't do anything since it's not a ref
		throw new Exception("Not cloneable");
	}

	// for REALLY long strings?
	/*
	public byte[] byteSequence
	{
		get
		{
			return Encoding.ASCII.GetBytes(letters);
		}
		set
		{
			letters = System.Text.Encoding.UTF8.GetString(byteSequence);
		}
	}
	*/
}
