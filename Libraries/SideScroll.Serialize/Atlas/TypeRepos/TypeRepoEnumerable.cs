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

	protected readonly Type? ElementType;
	protected TypeRepo? ListTypeRepo;
	protected readonly MethodInfo? AddMethod;

	private PropertyInfo? _countPropertyInfo; // IEnumerable isn't required to implement this

	public TypeRepoEnumerable(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
		if (LoadableType != null)
		{
			Type[] types = LoadableType.GetGenericArguments();
			if (types.Length > 0)
			{
				ElementType = types[0];
			}

			AddMethod = LoadableType.GetMethods()
				.FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);

			_countPropertyInfo = LoadableType.GetProperty("Count");
		}
	}

	/*public static bool CanAssign(Type type)
	{
		return type.IsGenericType && typeof(HashSet<>).IsAssignableFrom(type.GetGenericTypeDefinition());
	}*/

	public override void InitializeLoading(Log log)
	{
		if (ElementType != null)
		{
			ListTypeRepo = Serializer.GetOrCreateRepo(log, ElementType);
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
		var enumerable = (IEnumerable)obj;

		int count = (int)_countPropertyInfo!.GetValue(enumerable, null)!;
		writer.Write(count);
		foreach (object item in enumerable)
		{
			Serializer.WriteObjectRef(ElementType!, item, writer);
		}
	}

	public override void LoadObjectData(object obj)
	{
		int count = Reader!.ReadInt32();

		for (int j = 0; j < count; j++)
		{
			object objectValue = ListTypeRepo!.LoadObjectRef()!;
			AddMethod!.Invoke(obj, [objectValue]);
		}
	}

	public override void Clone(object source, object dest)
	{
		var enumerable = (IEnumerable)source;
		foreach (object? item in enumerable)
		{
			object? clone = Serializer.Clone(item);
			AddMethod!.Invoke(dest, [clone]);
		}
	}
}
