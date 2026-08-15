using SideScroll.Serialize.Atlas.Schema;
using System.Collections;

namespace SideScroll.Serialize.Atlas.TypeRepos;

/// <summary>
/// Loads the collections that don't implement <see cref="IList"/> or <see cref="IDictionary"/> and
/// aren't a <see cref="HashSet{T}"/>, which the repos before this one claim
/// </summary>
/// <remarks>
/// Without one of their own they were left to <see cref="TypeRepoObject"/>, which reaches a type's
/// contents through its properties. A collection doesn't expose its elements that way, so they
/// saved with their element count and reloaded empty, reporting success either way
/// </remarks>
public class TypeRepoCollection : TypeRepoEnumerable, IPreloadRepo
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
			{
				return new TypeRepoCollection(serializer, typeSchema);
			}
			return null;
		}
	}

	/// <summary>
	/// Gets or sets the collections handled here, matched against a type and the types it derives from
	/// </summary>
	/// <remarks>
	/// Named rather than inferred from having an add method, so a type that happens to be enumerable
	/// keeps being saved through its properties instead of silently losing them to this
	/// </remarks>
	public static HashSet<Type> SupportedTypes { get; set; } =
	[
		typeof(SortedSet<>),
		typeof(Queue<>),
		typeof(Stack<>),
		typeof(LinkedList<>),
	];

	// Enumerating a Stack yields the most recently pushed first, so pushing them back in that order
	// would reverse it on every load
	private readonly bool _reversesOnEnumeration;

	public TypeRepoCollection(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
		_reversesOnEnumeration = IsGenericType(LoadableType, typeof(Stack<>));
	}

	public static bool CanAssign(Type? type)
	{
		return SupportedTypes.Any(supported => IsGenericType(type, supported));
	}

	private static bool IsGenericType(Type? type, Type genericTypeDefinition)
	{
		for (Type? baseType = type; baseType != null && baseType != typeof(object); baseType = baseType.BaseType)
		{
			if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericTypeDefinition)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Reads the elements once before any are added, so they're complete when they're ordered
	/// </summary>
	/// <remarks>
	/// A <see cref="SortedSet{T}"/> compares each element as it's added, and one compared before its
	/// own members are read compares equal to every other element of its type, which the set then
	/// discards as a duplicate. The collections that keep insertion order don't need this, they just
	/// read the same bytes twice
	/// </remarks>
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

		var values = new object?[count];
		for (int j = 0; j < count; j++)
		{
			values[j] = ListTypeRepo!.LoadObjectRef();
		}

		AddAll(obj, values);
	}

	public override void Clone(object source, object dest)
	{
		object?[] values = ((IEnumerable)source)
			.Cast<object?>()
			.Select(Serializer.Clone)
			.ToArray();

		AddAll(dest, values);
	}

	private void AddAll(object obj, object?[] values)
	{
		if (_reversesOnEnumeration)
		{
			Array.Reverse(values);
		}

		foreach (object? value in values)
		{
			AddMethod!.Invoke(obj, [value]);
		}
	}
}
