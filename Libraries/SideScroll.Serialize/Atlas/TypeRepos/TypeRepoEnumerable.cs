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
			ElementType = GetElementType(LoadableType) ?? typeof(object);

			AddMethod = LoadableType.GetMethods()
				.FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);

			_countPropertyInfo = LoadableType.GetProperty("Count");
		}
	}

	/// <summary>
	/// Resolves the collection's element type, or null if it can't be determined
	/// </summary>
	private static Type? GetElementType(Type type)
	{
		// IEnumerable<T> names the element type directly, so prefer it over walking the base types.
		// A generic ancestor's first argument isn't always the element type (class Cache<TKey> : HashSet<string>)
		Type? enumerableType = type.GetInterfaces()
			.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		if (enumerableType != null)
		{
			return enumerableType.GetGenericArguments()[0];
		}

		// Only implements the non-generic IEnumerable, fall back to the nearest generic ancestor
		for (Type? baseType = type; baseType != null && baseType != typeof(object); baseType = baseType.BaseType)
		{
			if (baseType.IsGenericType && baseType.GetGenericArguments() is { Length: > 0 } arguments)
			{
				return arguments[0];
			}
		}

		return null;
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
		ValidateBytesAvailable(count);

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
