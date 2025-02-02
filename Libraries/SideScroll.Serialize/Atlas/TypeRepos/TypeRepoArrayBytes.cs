using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Collections;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoArrayBytes(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	private int[]? _sizes;

	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type))
			{
				return new TypeRepoArrayBytes(serializer, typeSchema);
			}
			return null;
		}
	}

	public static bool CanAssign(Type? type)
	{
		return typeof(byte[]).IsAssignableFrom(type);
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

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		var array = (byte[])obj;

		//writer.Write(array.Length);
		writer.Write(array);
	}

	protected override object? CreateObject(int objectIndex)
	{
		//int count = reader.ReadInt32();
		int count = _sizes![objectIndex];

		var array = new byte[count];
		ObjectsLoaded[objectIndex] = array;

		Serializer.QueueLoading(this, objectIndex);
		return array;
	}

	public override void LoadObjectData(object obj)
	{
		var array = (byte[])obj;
		Reader!.Read(array, 0, array.Length);
	}

	public override void Clone(object source, object dest)
	{
		var sourceBytes = (byte[])source;
		var destBytes = (byte[])dest;
		Array.Copy(sourceBytes, destBytes, sourceBytes.Length); // could use Buffer.BlockCopy but the performance is supposedly nearly identical
	}
}
