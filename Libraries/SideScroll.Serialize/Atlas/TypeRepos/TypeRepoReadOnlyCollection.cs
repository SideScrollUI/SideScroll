using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.TypeRepos;

/// <summary>
/// Loads a <see cref="ReadOnlyCollection{T}"/>, which has no parameterless constructor and can't be
/// added to once it exists
/// </summary>
/// <remarks>
/// It implements the non-generic <see cref="IList"/>, so <see cref="TypeRepoList"/> claimed it and
/// saved it, and loading failed while constructing it, leaving an empty collection behind. A
/// <see cref="ReadOnlyCollection{T}"/> is a view over the list it wraps rather than a copy of it, so
/// the collection is created around an empty list that the elements are then read into, which also
/// leaves it in place before they're read for one of them to reference it back
/// </remarks>
public class TypeRepoReadOnlyCollection : TypeRepo
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
			{
				return new TypeRepoReadOnlyCollection(serializer, typeSchema);
			}
			return null;
		}
	}

	private readonly Type? _elementType;
	private readonly Type? _backingListType;
	private readonly ConstructorInfo? _constructor;

	private TypeRepo? _elementTypeRepo;

	// The list each loaded collection wraps, so its elements can be added after it's constructed
	private readonly Dictionary<object, IList> _backingLists = new(ReferenceEqualityComparer.Instance);

	public TypeRepoReadOnlyCollection(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
		_elementType = GetElementType(LoadableType);
		if (_elementType != null)
		{
			_backingListType = typeof(List<>).MakeGenericType(_elementType);
			_constructor = GetListConstructor(LoadableType!, _elementType);
		}
	}

	public static bool CanAssign(Type? type)
	{
		if (GetElementType(type) is not { } elementType) return false;

		return GetListConstructor(type!, elementType) != null;
	}

	/// <summary>
	/// Returns the element type when the type is a <see cref="ReadOnlyCollection{T}"/> or derives
	/// from one, or null when it isn't
	/// </summary>
	private static Type? GetElementType(Type? type)
	{
		for (Type? baseType = type; baseType != null && baseType != typeof(object); baseType = baseType.BaseType)
		{
			if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>))
			{
				return baseType.GetGenericArguments()[0];
			}
		}
		return null;
	}

	/// <summary>
	/// Returns the constructor taking the list to wrap, or null for a subclass that doesn't have one
	/// </summary>
	private static ConstructorInfo? GetListConstructor(Type type, Type elementType)
	{
		return type.GetConstructor([typeof(IList<>).MakeGenericType(elementType)]);
	}

	public override void InitializeLoading(Log log)
	{
		if (_elementType != null)
		{
			_elementTypeRepo = Serializer.GetOrCreateRepo(log, _elementType);
		}
	}

	public override void AddChildObjects(object obj)
	{
		foreach (object? item in (IEnumerable)obj)
		{
			Serializer.AddObjectRef(item);
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		var collection = (IList)obj;

		writer.Write(collection.Count);
		foreach (object? item in collection)
		{
			Serializer.WriteObjectRef(_elementType!, item, writer);
		}
	}

	protected override object? CreateObject(int objectIndex)
	{
		var backingList = (IList)Activator.CreateInstance(_backingListType!)!;
		object obj = _constructor!.Invoke([backingList]);

		_backingLists[obj] = backingList;
		ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
		Serializer.QueueLoading(this, objectIndex);
		return obj;
	}

	public override void LoadObjectData(object obj)
	{
		int count = Reader!.ReadInt32();
		ValidateBytesAvailable(count);

		IList backingList = _backingLists[obj];
		for (int j = 0; j < count; j++)
		{
			backingList.Add(_elementTypeRepo!.LoadObjectRef());
		}
	}

	public override void Clone(object source, object dest)
	{
		IList backingList = _backingLists[dest];
		foreach (object? item in (IEnumerable)source)
		{
			backingList.Add(Serializer.Clone(item));
		}
	}

	/// <summary>
	/// Creates the collection a clone is copied into, which needs its list before it exists
	/// </summary>
	public object CreateClone()
	{
		var backingList = (IList)Activator.CreateInstance(_backingListType!)!;
		object obj = _constructor!.Invoke([backingList]);

		_backingLists[obj] = backingList;
		return obj;
	}
}
