using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Atlas.Serialize
{
	/*public class TypeRepoSerializable : TypeRepo
	{
		private IFormatter formatter = new BinaryFormatter();
		private MemoryStream memoryStream = new MemoryStream(); // for cloning

		public TypeRepoSerializable(TypeSchema typeSchema) : 
			base(typeSchema)
		{
		}

		public override void SaveHeader(BinaryWriter writer)
		{
			foreach (ISerializable iSerializable in objects)
			{
				formatter.Serialize(writer.BaseStream, iSerializable);
			}
		}

		public override void LoadHeader(BinaryReader reader)
		{
			// Can't use Activator because Array requires parameters in it's constructor
			for (int i = 0; i < typeSchema.numObjects; i++)
			{
				object obj = formatter.Deserialize(reader.BaseStream);
				objects.Add(obj);
			}
		}

		public override void CreateObjects()
		{
			// don't need to do anything

		}

		public override void AddChildObjects(object obj)
		{
		}

		protected override void SaveObjectData(BinaryWriter writer)
		{
		}
		
		protected override object LoadObjectData(BinaryReader reader, byte[] bytes, ref int byteOffset, int objectIndex)
		{
		}

		public override void Clone(object source, object dest)
		{
			/*byte[] iSource = (byte[])source;
			byte[] iDest = (byte[])dest;
			Array.Copy(iSource, iDest, iSource.Length); // could use Buffer.BlockCopy but the performance is supposedly nearly identical
			*//*
		}
	}*/
}
