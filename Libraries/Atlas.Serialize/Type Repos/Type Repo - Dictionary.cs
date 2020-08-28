using Atlas.Core;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Atlas.Serialize
{
	public class TypeRepoDictionary : TypeRepo
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoDictionary(serializer, typeSchema);
				return null;
			}
		}

		private Type typeKey;
		private Type typeValue;
		private TypeRepo list1TypeRepo;
		private TypeRepo list2TypeRepo;
		private MethodInfo addMethod;

		public TypeRepoDictionary(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
			Type[] types = type.GetGenericArguments();
			if (types.Length > 0)
			{
				typeKey = types[0];
				typeValue = types[1];
			}
			addMethod = type.GetMethods()
				.Where(m => m.Name == "Add" && m.GetParameters().Count() == 2).FirstOrDefault();
		}

		public static bool CanAssign(Type type)
		{
			return typeof(IDictionary).IsAssignableFrom(type);
		}

		public override void InitializeLoading(Log log)
		{
			// these base types might not be serialized
			if (typeKey != null)
				list1TypeRepo = serializer.GetOrCreateRepo(log, typeKey);
			if (typeValue != null)
				list2TypeRepo = serializer.GetOrCreateRepo(log, typeValue);
		}

		public override void AddChildObjects(object obj)
		{
			IDictionary dictionary = (IDictionary)obj;
			foreach (DictionaryEntry item in dictionary)
			{
				serializer.AddObjectRef(item.Key);
				serializer.AddObjectRef(item.Value);
			}
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			IDictionary dictionary = (IDictionary)obj;
			
			writer.Write(dictionary.Count);
			foreach (DictionaryEntry item in dictionary)
			{
				serializer.WriteObjectRef(typeKey, item.Key, writer);
				serializer.WriteObjectRef(typeValue, item.Value, writer);
			}
		}

		public override void LoadObjectData(object obj)
		{
			IDictionary iCollection = (IDictionary)obj;
			int count = reader.ReadInt32();

			for (int j = 0; j < count; j++)
			{
				object key = list1TypeRepo.LoadObjectRef();
				object value = list2TypeRepo.LoadObjectRef();

				if (key != null)
					addMethod.Invoke(iCollection, new object[] { key, value });
			}
		}

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			object obj = Activator.CreateInstance(type, true);
			objects[objectIndex] = obj; // must assign before loading any more refs

			IDictionary iCollection = (IDictionary)obj;
			int count = BitConverter.ToInt32(bytes, byteOffset);
			byteOffset += sizeof(int);

			for (int j = 0; j < count; j++)
			{
				object key = list1TypeRepo.LoadObjectRef(bytes, ref byteOffset);
				object value = list2TypeRepo.LoadObjectRef(bytes, ref byteOffset);

				if (key != null)
					addMethod.Invoke(iCollection, new object[] { key, value });
			}
			return obj;
		}

		public override void Clone(object source, object dest)
		{
			IDictionary iSource = (IDictionary)source;
			IDictionary iDest = (IDictionary)dest;
			foreach (DictionaryEntry item in iSource)
			{
				object key = serializer.Clone(item.Key);
				object value = serializer.Clone(item.Value);
				addMethod.Invoke(iDest, new object[] { key, value });
			}
		}
	}
}
