﻿using Atlas.Core;
using System;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoString : TypeRepo, IDisposable
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (typeSchema.Type == typeof(string))
					return new TypeRepoString(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoString(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
		}
		
		public override void InitializeLoading(Log log)
		{
			/*FileStream primaryStream = reader.BaseStream as FileStream;
			stream = new FileStream(primaryStream.Name, FileMode.Open, FileAccess.Read, FileShare.Read);
			localReader = new BinaryReader(stream);*/
		}

		public override void AddChildObjects(object obj)
		{
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			writer.Write((string)obj);
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			string value = System.Text.Encoding.UTF8.GetString(bytes);

			//string value = BitConverter.ToString(bytes, byteOffset);
			object obj = value;
			byteOffset += value.Length + 1; // 2?
			return obj;
		}

		protected override object CreateObject(int objectIndex)
		{
			long position = reader.BaseStream.Position;
			reader.BaseStream.Position = objectOffsets[objectIndex];

			object obj = reader.ReadString();
			reader.BaseStream.Position = position;

			objectsLoaded[objectIndex] = obj; // must assign before loading any more refs
			return obj;
		}

		public override void LoadObjectData(object obj)
		{
		}
		
		public override void Clone(object source, object dest)
		{
			// assigning won't do anything since it's not a ref
			throw new Exception("Not cloneable");
		}

		// for REALLY long strings?
		/*
		public byte[] byteSequence
		{
			get
			{
				return Encoding.ASCII.GetBytes(letters);
			}
			set
			{
				letters = System.Text.Encoding.UTF8.GetString(byteSequence);
			}
		}
		*/
	}
}
