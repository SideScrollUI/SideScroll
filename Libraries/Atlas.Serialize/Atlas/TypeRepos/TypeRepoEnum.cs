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
				if (CanAssign(typeSchema.Type))
					return new TypeRepoEnum(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoEnum(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
		}

		public static bool CanAssign(Type type)
		{
			return type.IsEnum;
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			writer.Write((int)obj);
		}

		protected override object CreateObject(int objectIndex)
		{
			long position = Reader.BaseStream.Position;
			Reader.BaseStream.Position = ObjectOffsets[objectIndex];

			object obj = null;
			try
			{
				if (LoadableType.IsEnum)
					obj = Enum.ToObject(TypeSchema.Type, Reader.ReadInt32());
				else
					throw new Exception("Unhandled primitive type");
			}
			catch (Exception)
			{
				//log.Add(e);
			}
			Reader.BaseStream.Position = position;

			ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
			return obj;
		}

		public override object LoadObject()
		{
			object obj = Enum.ToObject(TypeSchema.Type, Reader.ReadInt32());
			return obj;
		}

		public override void Clone(object source, object dest)
		{
			// assigning won't do anything since it's not a ref
			throw new Exception("Not cloneable");
		}
	}
}
