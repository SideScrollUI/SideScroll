namespace SideScroll.Serialize;

public class TypeRepoUnknown(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (typeSchema.Type == null)
				return new TypeRepoUnknown(serializer, typeSchema);
			return null;
		}
	}

	public class NoConstructorCreator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if ((!typeSchema.HasConstructor && !typeSchema.IsSerialized) ||
				(typeSchema.IsPublicOnly && !serializer.PublicOnly))
				return new TypeRepoUnknown(serializer, typeSchema);
			return null;
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
	}

	public override void Clone(object source, object dest)
	{
		// assigning won't do anything since it's not a ref
		throw new Exception("Not cloneable");
	}

	protected override object? CreateObject(int objectIndex)
	{
		return null;
	}
}
