using SideScroll.Serialize.Atlas.Schema;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoDateTimeOffset(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
			{
				return new TypeRepoDateTimeOffset(serializer, typeSchema);
			}
			return null;
		}
	}

	public static bool CanAssign(Type? type)
	{
		return type == typeof(DateTimeOffset);
	}

	// Offsets are limited to +/- 14 hours, so minutes always fit in a short
	private const int OffsetSize = sizeof(long) + sizeof(short);

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		var dateTimeOffset = (DateTimeOffset)obj;
		writer.Write(dateTimeOffset.UtcTicks);
		writer.Write((short)dateTimeOffset.Offset.TotalMinutes);
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

				// Earlier versions only stored the UTC ticks and lost the offset
				short offsetMinutes = ObjectSizes![objectIndex] >= OffsetSize
					? Reader.ReadInt16()
					: (short)0;

				var dateTime = new DateTime(ticks, DateTimeKind.Utc);
				obj = new DateTimeOffset(dateTime).ToOffset(TimeSpan.FromMinutes(offsetMinutes));
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

	// not called, it's a struct and a value
	public override void Clone(object source, object dest)
	{
	}
}
