﻿using Atlas.Core;
using System;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoDateTime : TypeRepo, IDisposable
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoDateTime(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoDateTime(Serializer serializer, TypeSchema typeSchema) : 
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
			return type == typeof(DateTime);
		}

		public override void AddChildObjects(object obj)
		{
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			DateTime dateTime = (DateTime)obj;
			writer.Write(dateTime.Ticks);
			writer.Write((byte)dateTime.Kind);
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			long value = BitConverter.ToInt64(bytes, byteOffset);
			object obj = new DateTime(value);
			byteOffset += sizeof(long);
			Objects[objectIndex] = obj;
			return obj;
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
					int kindValue = reader.ReadByte();
					//Enum.ToObject(typeof(DateTimeKind), kindValue);
					DateTime dateTime = new DateTime(ticks, (DateTimeKind)kindValue);
					obj = dateTime;
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