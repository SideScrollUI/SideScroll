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

	// Multi dimensional arrays prefix their data with each dimension's length so they can be
	// recreated with the right shape. Single dimension arrays only use the header count
	private readonly int _rank = typeSchema.Type!.GetArrayRank();

	private int LengthsSize => _rank * sizeof(int);

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

		if (_rank > 1)
		{
			for (int dimension = 0; dimension < _rank; dimension++)
			{
				writer.Write(array.GetLength(dimension));
			}
		}

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

		// The reader is still positioned in whatever type referenced this array,
		// so only the size can be checked here
		ValidateDataSize(count);

		Array array;
		if (_rank > 1)
		{
			array = Array.CreateInstance(_elementType, ReadLengths(objectIndex));
		}
		else
		{
			array = Array.CreateInstance(_elementType, count);
		}

		ObjectsLoaded[objectIndex] = array;
		Serializer.QueueLoading(this, objectIndex);

		return array;
	}

	private int[] ReadLengths(int objectIndex)
	{
		long position = Reader!.BaseStream.Position;
		Reader.BaseStream.Position = ObjectOffsets![objectIndex];

		int[] lengths = new int[_rank];
		for (int dimension = 0; dimension < _rank; dimension++)
		{
			lengths[dimension] = Reader.ReadInt32();
		}

		Reader.BaseStream.Position = position;
		return lengths;
	}

	public override void LoadObjectData(object obj)
	{
		var array = (Array)obj;
		ValidateBytesAvailable(array.Length);

		if (_rank > 1)
		{
			// Skip the lengths already read by CreateObject()
			Reader!.BaseStream.Position += LengthsSize;

			int[] indices = new int[_rank];
			for (int i = 0; i < array.Length; i++)
			{
				array.SetValue(_listTypeRepo!.LoadObjectRef(), indices);
				Increment(array, indices);
			}
			return;
		}

		var list = (IList)obj;
		for (int j = 0; j < list.Count; j++)
		{
			object? item = _listTypeRepo!.LoadObjectRef();
			list[j] = item;
		}
	}

	// Advances the last dimension first to match the order foreach() saved them in
	private static void Increment(Array array, int[] indices)
	{
		for (int dimension = indices.Length - 1; dimension >= 0; dimension--)
		{
			if (++indices[dimension] < array.GetLength(dimension))
				return;

			indices[dimension] = 0;
		}
	}

	public override void Clone(object source, object dest)
	{
		var sourceArray = (Array)source;
		var destArray = (Array)dest;

		if (_rank > 1)
		{
			int[] indices = new int[_rank];
			foreach (var item in sourceArray)
			{
				destArray.SetValue(Serializer.Clone(item), indices);
				Increment(destArray, indices);
			}
			return;
		}

		IList destList = (IList)dest;
		int i = 0;
		foreach (var item in sourceArray)
		{
			object? clone = Serializer.Clone(item);
			destList[i++] = clone;
		}
	}
}
