using SideScroll.Serialize.Atlas.Schema;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoDecimal(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
			{
				return new TypeRepoDecimal(serializer, typeSchema);
			}
			return null;
		}
	}

	public static bool CanAssign(Type? type)
	{
		return type == typeof(decimal);
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		writer.Write((decimal)obj);
	}

	protected override object? CreateObject(int objectIndex)
	{
		return Reader!.ReadDecimal();
	}

	public override void Clone(object source, object dest)
	{
		// assigning won't do anything since it's not a ref
		throw new SerializerException("Not cloneable");
	}
}
