using SideScroll.Serialize.Atlas.Schema;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoDateTime(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type!))
				return new TypeRepoDateTime(serializer, typeSchema);
			return null;
		}
	}

	public static bool CanAssign(Type type)
	{
		return type == typeof(DateTime);
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		DateTime dateTime = (DateTime)obj;
		writer.Write(dateTime.Ticks);
		writer.Write((byte)dateTime.Kind);
	}

	protected override object? CreateObject(int objectIndex)
	{
		long position = Reader!.BaseStream.Position;
		Reader.BaseStream.Position = ObjectOffsets![objectIndex];

		object? obj = null;
		try
		{
			if (CanAssign(LoadableType!))
			{
				long ticks = Reader.ReadInt64();
				int kindValue = Reader.ReadByte();
				//Enum.ToObject(typeof(DateTimeKind), kindValue);
				DateTime dateTime = new(ticks, (DateTimeKind)kindValue);
				obj = dateTime;
			}
			else
			{
				throw new Exception("Unhandled primitive type");
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
		object obj = Enum.ToObject(TypeSchema.Type!, Reader!.ReadInt32());
		return obj;
	}

	// not called, it's a struct and a value
	public override void Clone(object source, object dest)
	{
		//dest = new DateTime(((DateTime)source).Ticks, ((DateTime)source).Kind);
	}
}
