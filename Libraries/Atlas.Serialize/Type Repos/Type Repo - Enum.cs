using Atlas.Core;
using System;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoEnum : TypeRepo, IDisposable
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.type))
					return new TypeRepoEnum(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoEnum(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
		}

		public override void InitializeLoading(Log log)
		{
			/*FileStream primaryStream = reader.BaseStream as FileStream;
			stream = new FileStream(primaryStream.Name, FileMode.Open, FileAccess.Read, FileShare.Read);
			localReader = new BinaryReader(stream);*/
		}

		public static bool CanAssign(Type type)
		{
			return type.IsEnum;
		}

		public override void AddChildObjects(object obj)
		{
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			writer.Write((int)obj);
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			int value = BitConverter.ToInt32(bytes, byteOffset);
			object obj = Enum.ToObject(typeSchema.type, value);
			byteOffset += sizeof(int);
			objects[objectIndex] = obj;
			return obj;
		}

		protected override object CreateObject(int objectIndex)
		{
			long position = reader.BaseStream.Position;
			reader.BaseStream.Position = objectOffsets[objectIndex];

			object obj = null;
			try
			{
				if (type.IsEnum)
					obj = Enum.ToObject(typeSchema.type, reader.ReadInt32());
				else
					throw new Exception("Unhandled primitive type");
			}
			catch (Exception)
			{
				//log.Add(e.Message, new Tag("Exception", e));
			}
			reader.BaseStream.Position = position;

			objectsLoaded[objectIndex] = obj; // must assign before loading any more refs
			return obj;
		}

		public override void LoadObjectData(object obj)
		{
		}

		public override object LoadObject()
		{
			object obj = Enum.ToObject(typeSchema.type, reader.ReadInt32());
			return obj;
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset)
		{
			int value = BitConverter.ToInt32(bytes, byteOffset);
			object obj = Enum.ToObject(typeSchema.type, value);
			byteOffset += sizeof(int);
			return obj;
		}

		public override void Clone(object source, object dest)
		{
			// assigning won't do anything since it's not a ref
			throw new Exception("Not cloneable");
		}
	}
}
