using Atlas.Core;
using System;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoUnknown : TypeRepo
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (typeSchema.type == null)
					return new TypeRepoUnknown(serializer, typeSchema);
				return null;
			}
		}
		
		public class NoConstructorCreator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (!typeSchema.hasConstructor)
					return new TypeRepoUnknown(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoUnknown(Serializer serializer, TypeSchema typeSchema) :
			base(serializer, typeSchema)
		{
		}

		public override void InitializeLoading(Log log)
		{
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
		}

		public override void LoadObjectData(object obj)
		{
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			return null;
		}

		public override void Clone(object source, object dest)
		{
			// assigning won't do anything since it's not a ref
			throw new Exception("Not cloneable");
		}

		public override void AddChildObjects(object obj)
		{
		}
	}
}
