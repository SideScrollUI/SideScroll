using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Collections;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.TypeRepos;

// not used yet
public class TypeRepoCollection : TypeRepo
{
	/*public class Creator : IRepoCreator
	{
		public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.type))
				return new TypeRepoCollection(serializer, typeSchema);
			return null;
		}
	}*/

	private TypeRepo? _listTypeRepo;
	private readonly MethodInfo? _addMethod;
	private readonly Type? _elementType;

	public TypeRepoCollection(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
		Type[] types = LoadableType!.GetGenericArguments();
		if (types.Length > 0)
		{
			_elementType = types[0];
		}

		_addMethod = LoadableType.GetMethods()
			.FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);
	}

	public override void InitializeLoading(Log log)
	{
		if (_elementType != null)
		{
			_listTypeRepo = Serializer.GetOrCreateRepo(log, _elementType);
		}
	}

	public override void AddChildObjects(object obj)
	{
		var collection = (ICollection)obj;
		foreach (object? item in collection)
		{
			Serializer.AddObjectRef(item);
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		var collection = (ICollection)obj;

		writer.Write(collection.Count);
		foreach (object? item in collection)
		{
			Serializer.WriteObjectRef(_elementType!, item, writer);
		}
	}

	public override void LoadObjectData(object obj)
	{
		//(ICollection<listTypeRepo.type>)objects[i];
		int count = Reader!.ReadInt32();
		for (int j = 0; j < count; j++)
		{
			object? objectValue = _listTypeRepo!.LoadObjectRef();
			_addMethod!.Invoke(obj, [objectValue]);
		}
	}

	public override void Clone(object source, object dest)
	{
		var iSource = (ICollection)source;
		var iDest = (ICollection)dest;
		foreach (object? item in iSource)
		{
			object? clone = Serializer.Clone(item);
			_addMethod!.Invoke(iDest, [clone]);
		}
	}
}
