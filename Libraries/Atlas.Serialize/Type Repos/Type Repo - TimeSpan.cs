using Atlas.Core;
using System;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoTimeSpan : TypeRepo, IDisposable
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoTimeSpan(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoTimeSpan(Serializer serializer, TypeSchema typeSchema) : 
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
			return type == typeof(TimeSpan);
		}

		public override void AddChildObjects(object obj)
		{
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			TimeSpan timeSpan = (TimeSpan)obj;
			writer.Write(timeSpan.Ticks);
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			long ticks = BitConverter.ToInt64(bytes, byteOffset);
			TimeSpan timeSpan = new TimeSpan(ticks);
			byteOffset += sizeof(long);
			Objects[objectIndex] = timeSpan;
			return timeSpan;
		}

		protected override object CreateObject(int objectIndex)
		{
			long position = reader.BaseStream.Position;
			reader.BaseStream.Position = ObjectOffsets[objectIndex];

			object obj = null;
			try
			{
				if (CanAssign(Type))
				{
					long ticks = reader.ReadInt64();
					obj = new TimeSpan(ticks);
				}
				else
				{
					throw new Exception("Unhandled primitive type");
				}
			}
			catch (Exception)
			{
				//log.Add(e);
			}
			reader.BaseStream.Position = position;

			ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
			return obj;
		}

		public override void LoadObjectData(object obj)
		{
		}

		public override object LoadObject()
		{
			object obj = Enum.ToObject(TypeSchema.Type, reader.ReadInt32());
			return obj;
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset)
		{
			int value = BitConverter.ToInt32(bytes, byteOffset);
			object obj = Enum.ToObject(TypeSchema.Type, value);
			byteOffset += sizeof(int);
			return obj;
		}

		// not called, it's a struct and a value
		public override void Clone(object source, object dest)
		{
			//dest = new DateTime(((DateTime)source).Ticks, ((DateTime)source).Kind);
		}
	}
}