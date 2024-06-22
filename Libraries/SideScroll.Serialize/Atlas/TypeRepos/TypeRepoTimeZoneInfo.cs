namespace SideScroll.Serialize;

public class TypeRepoTimeZoneInfo(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type!))
				return new TypeRepoTimeZoneInfo(serializer, typeSchema);
			return null;
		}
	}

	public static bool CanAssign(Type type)
	{
		return type == typeof(TimeZoneInfo);
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		TimeZoneInfo timeZoneInfo = (TimeZoneInfo)obj;
		writer.Write(timeZoneInfo.Id);
	}

	protected override object? CreateObject(int objectIndex)
	{
		long position = Reader!.BaseStream.Position;
		Reader.BaseStream.Position = ObjectOffsets![objectIndex];

		string serializedString = Reader.ReadString();
		TimeZoneInfo? timeZoneInfo = LoadTimeZoneInfo(serializedString);
		Reader.BaseStream.Position = position;

		ObjectsLoaded[objectIndex] = timeZoneInfo; // must assign before loading any more refs
		return timeZoneInfo;
	}

	public override object? LoadObject()
	{
		string serializedString = Reader!.ReadString();
		TimeZoneInfo? timeZoneInfo = LoadTimeZoneInfo(serializedString);
		return timeZoneInfo;
	}

	// not called, it's a struct and a value
	public override void Clone(object source, object dest)
	{
		//dest = new DateTime(((DateTime)source).Ticks, ((DateTime)source).Kind);
	}

	private static TimeZoneInfo? LoadTimeZoneInfo(string serializedString)
	{
		string id = serializedString.Split(';', 2).First(); // deprecated format has multiple fields
		try
		{
			return TimeZoneInfo.FindSystemTimeZoneById(id);
		}
		catch (Exception)
		{
		}
		return null;
	}
}
