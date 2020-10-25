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
				if (typeof(Type).IsAssignableFrom(typeSchema.Type))
					return new TypeRepoType(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoType(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			writer.Write(((Type)obj).AssemblyQualifiedName);
		}

		protected override object CreateObject(int objectIndex)
		{
			long position = Reader.BaseStream.Position;
			Reader.BaseStream.Position = ObjectOffsets[objectIndex];
			
			string assemblyQualifiedName = Reader.ReadString();
			object obj = Type.GetType(assemblyQualifiedName, false);
			Reader.BaseStream.Position = position;

			ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
			return obj;
		}

		public override void Clone(object source, object dest)
		{
			// assigning won't do anything since it's not a ref
			throw new Exception("Not cloneable");
		}
	}
}
