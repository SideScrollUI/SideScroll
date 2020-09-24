using Atlas.Core;
using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace Atlas.Serialize
{
	public class TypeRepoList : TypeRepo
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoList(serializer, typeSchema);
				return null;
			}
		}

		private TypeRepo listTypeRepo;
		private PropertyInfo propertyInfoCapacity;
		private Type elementType;

		public TypeRepoList(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
			Type[] types = Type.GetGenericArguments();
			if (types.Length > 0)
				elementType = types[0];
		}

		public static bool CanAssign(Type type)
		{
			return typeof(IList).IsAssignableFrom(type);
		}

		public override void InitializeLoading(Log log)
		{
			if (elementType != null)
				listTypeRepo = Serializer.GetOrCreateRepo(log, elementType);
			
			propertyInfoCapacity = LoadableType.GetProperty("Capacity");
		}

		public override void AddChildObjects(object obj)
		{
			IList iList = (IList)obj;
			foreach (var item in iList)
			{
				//if (item.GetType() != elementType)
				//	typeSchema.hasSubType = true;
				Serializer.AddObjectRef(item);
			}
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			IList iList = (IList)obj;
			
			writer.Write(iList.Count);
			foreach (var item in iList)
			{
				Serializer.WriteObjectRef(elementType, item, writer);
			}
		}

		public override void LoadObjectData(object obj)
		{
			IList iList = (IList)obj;
			int count = Reader.ReadInt32();
			if (propertyInfoCapacity != null)
				propertyInfoCapacity.SetValue(iList, count);

			for (int j = 0; j < count; j++)
			{
				object valueObject = listTypeRepo.LoadObjectRef();
				iList.Add(valueObject);
			}
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			object obj = Activator.CreateInstance(LoadableType, true);
			Objects[objectIndex] = obj; // must assign before loading any more refs

			IList iList = (IList)obj;
			int count = BitConverter.ToInt32(bytes, byteOffset);
			byteOffset += sizeof(int);

			for (int j = 0; j < count; j++)
			{
				object valueObject = listTypeRepo.LoadObjectRef(bytes, ref byteOffset);
				iList.Add(valueObject);
			}
			return obj;
		}

		public override void Clone(object source, object dest)
		{
			IList iSource = (IList)source;
			IList iDest = (IList)dest;
			foreach (var item in iSource)
			{
				object clone = Serializer.Clone(item);
				iDest.Add(clone);
			}
		}
	}
}
