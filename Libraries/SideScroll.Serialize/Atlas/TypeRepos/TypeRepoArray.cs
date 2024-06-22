using SideScroll.Logs;
using System.Collections;

namespace SideScroll.Serialize;

public class TypeRepoArray(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type!))
				return new TypeRepoArray(serializer, typeSchema);
			return null;
		}
	}

	private TypeRepo? _listTypeRepo;
	private int[]? _sizes;
	private readonly Type _elementType = typeSchema.Type!.GetElementType()!;

	public static bool CanAssign(Type type)
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
		Array array = (Array)obj;
		foreach (var item in array)
		{
			Serializer.AddObjectRef(item);
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		Array array = (Array)obj;

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

		Array array = Array.CreateInstance(TypeSchema.Type!.GetElementType()!, count);
		ObjectsLoaded[objectIndex] = array;
		Serializer.QueueLoading(this, objectIndex);

		return array;
	}

	public override void LoadObjectData(object obj)
	{
		// Can't use Activator because Array requires parameters in it's constructor

		IList iList = (IList)obj;

		for (int j = 0; j < iList.Count; j++)
		{
			object? item = _listTypeRepo?.LoadObjectRef();
			iList[j] = item;
		}
	}

	public override void Clone(object source, object dest)
	{
		Array iSource = (Array)source;
		IList iDest = (IList)dest;
		int i = 0;
		foreach (var item in iSource)
		{
			object? clone = Serializer.Clone(item);
			iDest[i++] = clone;
		}
	}
}
