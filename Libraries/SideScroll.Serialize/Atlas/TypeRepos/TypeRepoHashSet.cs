using SideScroll.Serialize.Atlas.Schema;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoHashSet : TypeRepoEnumerable, IPreloadRepo
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
			{
				return new TypeRepoHashSet(serializer, typeSchema);
			}
			return null;
		}
	}

	public TypeRepoHashSet(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
	}

	public static bool CanAssign(Type? type)
	{
		if (type == null) return false;
		
		Type? baseType = type;
		while (baseType != null && baseType != typeof(object))
		{
			if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(HashSet<>))
			{
				return true;
			}
			baseType = baseType.BaseType;
		}
		
		return false;
	}

	// Preload the items first so they get unique hash codes before adding to the HashSet
	// Otherwise only a single item will get added since they'll all have default values
	public void PreloadObjectData(object? obj)
	{
		int count = Reader!.ReadInt32();
		ValidateBytesAvailable(count);

		for (int j = 0; j < count; j++)
		{
			ListTypeRepo!.LoadObjectRef();
		}
	}

	public override void LoadObjectData(object obj)
	{
		int count = Reader!.ReadInt32();
		ValidateBytesAvailable(count);

		for (int j = 0; j < count; j++)
		{
			object? objectValue = ListTypeRepo!.LoadObjectRef();
			AddMethod!.Invoke(obj, [objectValue]);
		}
	}
}
