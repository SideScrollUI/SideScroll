using System;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoVersion : TypeRepo, IDisposable
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoVersion(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoVersion(Serializer serializer, TypeSchema typeSchema) :
			base(serializer, typeSchema)
		{
		}

		public static bool CanAssign(Type type)
		{
			return type == typeof(Version);
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			writer.Write(((Version)obj).ToString());
		}

		protected override object CreateObject(int objectIndex)
		{
			long position = Reader.BaseStream.Position;
			Reader.BaseStream.Position = ObjectOffsets[objectIndex];

			string version = Reader.ReadString();
			object obj = new Version(version);
			Reader.BaseStream.Position = position;

			ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
			return obj;
		}

		public override object LoadObject()
		{
			object obj = new Version(Reader.ReadString());
			return obj;
		}

		public override void Clone(object source, object dest)
		{
			//dest = ((Version)source).Clone();
		}
	}
}
