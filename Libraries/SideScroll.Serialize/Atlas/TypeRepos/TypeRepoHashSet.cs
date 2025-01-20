using SideScroll.Serialize.Atlas.Schema;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoHashSet : TypeRepoEnumerable, IPreloadRepo
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type!))
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

	public static bool CanAssign(Type type)
	{
		return type.IsGenericType && typeof(HashSet<>).IsAssignableFrom(type.GetGenericTypeDefinition());
	}

	// Preload the items first so they get unique hash codes before adding to the HashSet
	// Otherwise only a single item will get added since they'll all have default values
	public void PreloadObjectData(object? obj)
	{
		int count = Reader!.ReadInt32();

		for (int j = 0; j < count; j++)
		{
			_listTypeRepo!.LoadObjectRef();
		}
	}

	public override void LoadObjectData(object obj)
	{
		int count = Reader!.ReadInt32();

		for (int j = 0; j < count; j++)
		{
			object? objectValue = _listTypeRepo!.LoadObjectRef();
			_addMethod!.Invoke(obj, [objectValue]);
		}
	}
}
