using Atlas.Core;
using System;
using System.Diagnostics;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoPrimitive : TypeRepo, IDisposable
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoPrimitive(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoPrimitive(Serializer serializer, TypeSchema typeSchema) : 
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
			return type.IsPrimitive;
		}

		public override void AddChildObjects(object obj)
		{
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			if (obj is uint u)
				writer.Write(u);
			else if (obj is int i)
				writer.Write(i);
			else if (obj is long l)
				writer.Write(l);
			else if (obj is double d)
				writer.Write(d);
			else if (obj is float f)
				writer.Write((double)f); // there's no ReadFloat() routine
			else if (obj is bool b)
				writer.Write(b); // 1 byte
			else if (obj is char c)
				writer.Write(c); // there's no ReadFloat() routine

			else
				Debug.Assert(true);
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			return null;
		}

		protected override object CreateObject(int objectIndex)
		{
			return null;
		}

		public override void LoadObjectData(object obj)
		{
		}

		public override object LoadObject()
		{
			object obj = null;
			if (type == typeof(uint))
				obj = reader.ReadUInt32();
			else if (type == typeof(int))
				obj = reader.ReadInt32();
			else if (type == typeof(long))
				obj = reader.ReadInt64();
			else if (type == typeof(double))
				obj = reader.ReadDouble();
			else if (type == typeof(float))
				obj = (float)reader.ReadDouble();
			else if (type == typeof(bool))
				obj = reader.ReadBoolean();
			else if (type == typeof(char))
				obj = reader.ReadChar();
			else
				throw new Exception("Unhandled primitive type");

			return obj;
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset)
		{
			object obj = null;
			if (type == typeof(uint))
			{
				obj = BitConverter.ToUInt32(bytes, byteOffset);
				byteOffset += sizeof(uint);
			}
			else if (type == typeof(int))
			{
				obj = BitConverter.ToInt32(bytes, byteOffset);
				byteOffset += sizeof(int);
			}
			else if (type == typeof(double))
			{
				obj = BitConverter.ToDouble(bytes, byteOffset);
				byteOffset += sizeof(double);
			}
			else if (type == typeof(bool))
			{
				obj = BitConverter.ToBoolean(bytes, byteOffset);
				byteOffset += sizeof(bool);
			}
			else
				throw new Exception("Unhandled primitive type");
			return obj;
		}

		public override void Clone(object source, object dest)
		{
			// assigning won't do anything since it's not a ref
			throw new Exception("Not cloneable");
		}
	}
}
