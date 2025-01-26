using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoList : TypeRepo
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type!))
			{
				return new TypeRepoList(serializer, typeSchema);
			}
			return null;
		}
	}

	private TypeRepo? _listTypeRepo;
	private PropertyInfo? _propertyInfoCapacity;
	private readonly Type? _elementType;

	public TypeRepoList(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
		Type[] types = LoadableType!
			.GetInterfaces()
			.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>))?
			.GetGenericArguments() ?? LoadableType.GetGenericArguments();

		if (types.Length > 0)
		{
			_elementType = types[0];
		}
		else
		{
			Debug.WriteLine($"Failed to find generic argument for {LoadableType}");
		}
	}

	public static bool CanAssign(Type type)
	{
		return typeof(IList).IsAssignableFrom(type);
	}

	public override void InitializeLoading(Log log)
	{
		if (_elementType != null)
		{
			_listTypeRepo = Serializer.GetOrCreateRepo(log, _elementType);
		}

		_propertyInfoCapacity = LoadableType!.GetProperty("Capacity");
	}

	public override void AddChildObjects(object obj)
	{
		var list = (IList)obj;
		foreach (var item in list)
		{
			//if (item.GetType() != elementType)
			//	typeSchema.hasSubType = true;
			Serializer.AddObjectRef(item);
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		var list = (IList)obj;

		writer.Write(list.Count);
		foreach (var item in list)
		{
			Serializer.WriteObjectRef(_elementType!, item, writer);
		}
	}

	public override void LoadObjectData(object obj)
	{
		var list = (IList)obj;
		int count = Reader!.ReadInt32();
		_propertyInfoCapacity?.SetValue(list, count);

		for (int j = 0; j < count; j++)
		{
			object? valueObject = _listTypeRepo!.LoadObjectRef();
			list.Add(valueObject);
		}
	}

	public override void Clone(object source, object dest)
	{
		var iSource = (IList)source;
		var iDest = (IList)dest;
		foreach (var item in iSource)
		{
			object? clone = Serializer.Clone(item);
			iDest.Add(clone);
		}
	}
}
