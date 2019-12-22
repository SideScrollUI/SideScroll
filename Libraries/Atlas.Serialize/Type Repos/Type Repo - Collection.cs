using Atlas.Core;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Atlas.Serialize
{
	// not used yet
	public class TypeRepoCollection : TypeRepo
	{
		/*public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.type))
					return new TypeRepoCollection(serializer, typeSchema);
				return null;
			}
		}*/

		private TypeRepo listTypeRepo;
		private MethodInfo addMethod;
		private Type elementType;

		public TypeRepoCollection(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
			Type[] types = type.GetGenericArguments();
			if (types.Length > 0)
				elementType = types[0];

			addMethod = type.GetMethods()
				.Where(m => m.Name == "Add" && m.GetParameters().Count() == 1).FirstOrDefault();
		}

		public override void InitializeLoading(Log log)
		{
			if (elementType != null)
				listTypeRepo = serializer.GetOrCreateRepo(log, elementType);
		}

		public override void AddChildObjects(object obj)
		{
			ICollection iCollection = (ICollection)obj;
			foreach (var item in iCollection)
			{
				serializer.AddObjectRef(item);
			}
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			ICollection iCollection = (ICollection)obj;
			
			writer.Write(iCollection.Count);
			foreach (var item in iCollection)
			{
				serializer.WriteObjectRef(elementType, item, writer);
			}
		}

		public override void LoadObjectData(object obj)
		{
			//(ICollection<listTypeRepo.type>)objects[i];
			int count = reader.ReadInt32();
			for (int j = 0; j < count; j++)
			{
				object objectValue = listTypeRepo.LoadObjectRef();
				addMethod.Invoke(obj, new object[] { objectValue });
			}
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			object obj = Activator.CreateInstance(type, true);
			objects[objectIndex] = obj; // must assign before loading any more refs

			//(ICollection<listTypeRepo.type>)objects[i];
			int count = BitConverter.ToInt32(bytes, byteOffset);
			byteOffset += sizeof(int);
			for (int j = 0; j < count; j++)
			{
				object objectValue = listTypeRepo.LoadObjectRef(bytes, ref byteOffset);
				addMethod.Invoke(obj, new object[] { objectValue });
			}
			return obj;
		}

		public override void Clone(object source, object dest)
		{
			ICollection iSource = (ICollection)source;
			ICollection iDest = (ICollection)dest;
			foreach (var item in iSource)
			{
				object clone = serializer.Clone(item);
				addMethod.Invoke(iDest, new object[] { clone });
			}
		}
	}
}
/*

*/