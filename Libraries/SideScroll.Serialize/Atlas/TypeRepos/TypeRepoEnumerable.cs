using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Collections;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoEnumerable : TypeRepo
{
	/*public class Creator : IRepoCreator
	{
		public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
				return new TypeRepoEnumerable(serializer, typeSchema);
			return null;
		}
	}*/

	protected readonly Type? _elementType;
	protected TypeRepo? _listTypeRepo;
	protected readonly MethodInfo? _addMethod;

	public TypeRepoEnumerable(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
		if (LoadableType != null)
		{
			Type[] types = LoadableType.GetGenericArguments();
			if (types.Length > 0)
			{
				_elementType = types[0];
			}

			_addMethod = LoadableType.GetMethods()
				.FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);
		}
	}

	/*public static bool CanAssign(Type type)
	{
		return type.IsGenericType && typeof(HashSet<>).IsAssignableFrom(type.GetGenericTypeDefinition());
	}*/

	public override void InitializeLoading(Log log)
	{
		if (_elementType != null)
		{
			_listTypeRepo = Serializer.GetOrCreateRepo(log, _elementType);
		}
	}

	public override void AddChildObjects(object obj)
	{
		var enumerable = (IEnumerable)obj;
		foreach (object? item in enumerable)
		{
			Serializer.AddObjectRef(item);
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		PropertyInfo propertyInfo = LoadableType!.GetProperty("Count")!; // IEnumerable isn't required to implement this
		var enumerable = (IEnumerable)obj;

		int count = (int)propertyInfo.GetValue(enumerable, null)!;
		writer.Write(count);
		foreach (object? item in enumerable)
		{
			Serializer.WriteObjectRef(_elementType!, item, writer);
		}
	}

	public override void LoadObjectData(object obj)
	{
		//(IEnumerable<listTypeRepo.type>)objects[i];
		int count = Reader!.ReadInt32();

		for (int j = 0; j < count; j++)
		{
			object objectValue = _listTypeRepo!.LoadObjectRef()!;
			_addMethod!.Invoke(obj, [objectValue]);
		}
	}

	public override void Clone(object source, object dest)
	{
		var enumerable = (IEnumerable)source;
		foreach (object? item in enumerable)
		{
			object? clone = Serializer.Clone(item);
			_addMethod!.Invoke(dest, [clone]);
		}
	}
}
