using SideScroll.Serialize.Atlas.Schema;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoDateTimeOffset(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type!))
			{
				return new TypeRepoDateTimeOffset(serializer, typeSchema);
			}
			return null;
		}
	}

	public static bool CanAssign(Type type)
	{
		return type == typeof(DateTimeOffset);
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		DateTime dateTime = ((DateTimeOffset)obj).UtcDateTime;
		writer.Write(dateTime.Ticks);
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
				var dateTime = new DateTime(ticks, DateTimeKind.Utc);
				obj = new DateTimeOffset(dateTime);
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
		object obj = Enum.ToObject(TypeSchema.Type!, Reader!.ReadInt32());
		return obj;
	}

	// not called, it's a struct and a value
	public override void Clone(object source, object dest)
	{
	}
}
