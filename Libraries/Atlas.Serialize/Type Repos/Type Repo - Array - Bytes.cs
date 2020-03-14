using Atlas.Core;
using System;
using System.Collections;
using System.IO;

namespace Atlas.Serialize
{
	public interface IRepoCreator
	{
		// needs to handle generics (lists, arrays, dictionaries)
		TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema);
	}


	public class TypeRepoArrayBytes : TypeRepo
	{
		private int[] sizes;

		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.type))
					return new TypeRepoArrayBytes(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoArrayBytes(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
		}

		public static bool CanAssign(Type type)
		{
			return typeof(byte[]).IsAssignableFrom(type);
		}

		public override void InitializeLoading(Log log)
		{
		}

		public override void SaveCustomHeader(BinaryWriter writer)
		{
			foreach (IList list in objects)
			{
				writer.Write((int)list.Count);
			}
		}

		public override void LoadCustomHeader()
		{
			sizes = new int[typeSchema.NumObjects];
			for (int i = 0; i < typeSchema.NumObjects; i++)
			{
				int count = reader.ReadInt32();
				sizes[i] = count;
			}
		}

		public override void AddChildObjects(object obj)
		{
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			byte[] array = (byte[])obj;
			
			//writer.Write(array.Length);
			writer.Write(array);
		}

		protected override object CreateObject(int objectIndex)
		{
			//int count = reader.ReadInt32();
			int count = sizes[objectIndex];

			byte[] array = new byte[count];
			objectsLoaded[objectIndex] = array;

			serializer.QueueLoading(this, objectIndex);
			return array;
		}

		public override void LoadObjectData(object obj)
		{
			byte[] array = (byte[])obj;
			reader.Read(array, 0, array.Length);
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			int count = BitConverter.ToInt32(bytes, byteOffset);
			byteOffset += sizeof(int);
			byte[] array = new byte[count];
			objectsLoaded[objectIndex] = array;

			//Array.Copy(bytes, byteOffset, array, count);
			Buffer.BlockCopy(bytes, byteOffset, array, 0, count);
			byteOffset += count;
			return array;
		}

		public override void Clone(object source, object dest)
		{
			byte[] iSource = (byte[])source;
			byte[] iDest = (byte[])dest;
			Array.Copy(iSource, iDest, iSource.Length); // could use Buffer.BlockCopy but the performance is supposedly nearly identical
		}
	}
}
