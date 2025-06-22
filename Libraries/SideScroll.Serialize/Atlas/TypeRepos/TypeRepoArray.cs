using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Collections;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoArray(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
			{
				return new TypeRepoArray(serializer, typeSchema);
			}
			return null;
		}
	}

	private TypeRepo? _listTypeRepo;
	private int[]? _sizes;
	private readonly Type _elementType = typeSchema.Type!.GetElementType()!;

	public static bool CanAssign(Type? type)
	{
		return typeof(Array).IsAssignableFrom(type);
	}

	public override void InitializeLoading(Log log)
	{
		_listTypeRepo = Serializer.GetOrCreateRepo(log, _elementType);
	}

	protected override void SaveCustomHeader(BinaryWriter writer)
	{
		foreach (IList list in Objects)
		{
			writer.Write((int)list.Count);
		}
	}

	protected override void LoadCustomHeader()
	{
		_sizes = new int[TypeSchema.NumObjects];
		for (int i = 0; i < TypeSchema.NumObjects; i++)
		{
			int count = Reader!.ReadInt32();
			_sizes[i] = count;
		}
	}

	public override void AddChildObjects(object obj)
	{
		var array = (Array)obj;
		foreach (var item in array)
		{
			Serializer.AddObjectRef(item);
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		var array = (Array)obj;

		//writer.Write(array.Length);
		foreach (var item in array)
		{
			Serializer.WriteObjectRef(_elementType, item, writer);
		}
	}

	protected override object? CreateObject(int objectIndex)
	{
		// Can't use Activator because Array requires parameters in it's constructor
		//int count = reader.ReadInt32();
		int count = _sizes![objectIndex];
		ValidateBytesAvailable(count);

		Array array = Array.CreateInstance(TypeSchema.Type!.GetElementType()!, count);
		ObjectsLoaded[objectIndex] = array;
		Serializer.QueueLoading(this, objectIndex);

		return array;
	}

	public override void LoadObjectData(object obj)
	{
		var list = (IList)obj;

		for (int j = 0; j < list.Count; j++)
		{
			object? item = _listTypeRepo?.LoadObjectRef();
			list[j] = item;
		}
	}

	public override void Clone(object source, object dest)
	{
		Array sourceArray = (Array)source;
		IList destList = (IList)dest;
		int i = 0;
		foreach (var item in sourceArray)
		{
			object? clone = Serializer.Clone(item);
			destList[i++] = clone;
		}
	}
}
