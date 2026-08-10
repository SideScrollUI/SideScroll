using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Collections;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoList : TypeRepo
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
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
			_elementType = typeof(object);
		}
	}

	public static bool CanAssign(Type? type)
	{
		return typeof(IList).IsAssignableFrom(type);
	}

	public override void InitializeLoading(Log log)
	{
		if (_elementType != null)
		{
			_listTypeRepo = Serializer.GetOrCreateRepo(log, _elementType);
		}

		_propertyInfoCapacity = GetCapacityProperty(LoadableType!);
	}

	/// <summary>
	/// Returns a writable Capacity property to preallocate with, or null when there isn't a usable one
	/// </summary>
	/// <remarks>
	/// A subclass declaring its own Capacity of a different type than the one it hides leaves
	/// GetProperty() unable to choose between them, which threw before the list could be read
	/// </remarks>
	private static PropertyInfo? GetCapacityProperty(Type type)
	{
		try
		{
			PropertyInfo? propertyInfo = type.GetProperty("Capacity");
			return propertyInfo?.CanWrite == true ? propertyInfo : null;
		}
		catch (AmbiguousMatchException)
		{
			return null;
		}
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
		ValidateBytesAvailable(count);
		SetCapacity(list, count);

		for (int j = 0; j < count; j++)
		{
			object? valueObject = _listTypeRepo!.LoadObjectRef();
			list.Add(valueObject);
		}
	}

	/// <summary>
	/// Preallocates the list for the number of elements about to be added
	/// </summary>
	/// <remarks>
	/// Only an optimization, so a Capacity that refuses the value still has to leave the elements
	/// loading. A fixed size list throws NotSupportedException, and a list whose constructor added
	/// more elements than were saved throws ArgumentOutOfRangeException for shrinking below its
	/// Count. Both used to abandon the rest of the list
	/// </remarks>
	private void SetCapacity(IList list, int count)
	{
		if (_propertyInfoCapacity == null) return;

		try
		{
			_propertyInfoCapacity.SetValue(list, count);
		}
		catch (Exception)
		{
		}
	}

	public override void Clone(object source, object dest)
	{
		var sourceList = (IList)source;
		var destList = (IList)dest;
		foreach (var item in sourceList)
		{
			object? clone = Serializer.Clone(item);
			destList.Add(clone);
		}
	}
}
