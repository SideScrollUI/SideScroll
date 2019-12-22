using Atlas.Core;
using System;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoType : TypeRepo, IDisposable
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (typeof(Type).IsAssignableFrom(typeSchema.type))
					return new TypeRepoType(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoType(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
		}

		public override void InitializeLoading(Log log)
		{
		}

		public override void AddChildObjects(object obj)
		{
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			writer.Write(((Type)obj).AssemblyQualifiedName);
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			string assemblyQualifiedName = BitConverter.ToString(bytes, byteOffset);
			object obj = Type.GetType(assemblyQualifiedName, false);
			byteOffset += assemblyQualifiedName.Length + 1; // 2?
			return obj;
		}

		protected override object CreateObject(int objectIndex)
		{
			long position = reader.BaseStream.Position;
			reader.BaseStream.Position = objectOffsets[objectIndex];
			
			string assemblyQualifiedName = reader.ReadString();
			object obj = Type.GetType(assemblyQualifiedName, false);
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
	}
}
/*

*/