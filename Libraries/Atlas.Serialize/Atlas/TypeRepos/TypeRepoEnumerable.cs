using Atlas.Core;
using System.Collections;
using System.Reflection;

namespace Atlas.Serialize;

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
				_elementType = types[0];

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
		IEnumerable iEnumerable = (IEnumerable)obj;
		foreach (var item in iEnumerable)
		{
			Serializer.AddObjectRef(item);
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		PropertyInfo propertyInfo = LoadableType!.GetProperty("Count")!; // IEnumerable isn't required to implement this
		IEnumerable iEnumerable = (IEnumerable)obj;

		int count = (int)propertyInfo.GetValue(iEnumerable, null)!;
		writer.Write(count);
		foreach (object item in iEnumerable)
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
		IEnumerable iSource = (IEnumerable)source;
		foreach (var item in iSource)
		{
			object? clone = Serializer.Clone(item);
			_addMethod!.Invoke(dest, [clone]);
		}
	}
}
