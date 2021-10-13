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

		private Type _typeKey;
		private Type _typeValue;

		private TypeRepo _list1TypeRepo;
		private TypeRepo _list2TypeRepo;

		private MethodInfo _addMethod;

		public TypeRepoDictionary(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
			Type[] types = LoadableType.GetGenericArguments();
			if (types.Length > 0)
			{
				_typeKey = types[0];
				_typeValue = types[1];
			}

			_addMethod = LoadableType.GetMethods()
				.Where(m => m.Name == "Add" && m.GetParameters().Count() == 2).FirstOrDefault();
		}

		public static bool CanAssign(Type type)
		{
			return typeof(IDictionary).IsAssignableFrom(type);
		}

		public override void InitializeLoading(Log log)
		{
			// these base types might not be serialized
			if (_typeKey != null)
				_list1TypeRepo = Serializer.GetOrCreateRepo(log, _typeKey);

			if (_typeValue != null)
				_list2TypeRepo = Serializer.GetOrCreateRepo(log, _typeValue);
		}

		public override void AddChildObjects(object obj)
		{
			IDictionary dictionary = (IDictionary)obj;
			foreach (DictionaryEntry item in dictionary)
			{
				Serializer.AddObjectRef(item.Key);
				Serializer.AddObjectRef(item.Value);
			}
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			IDictionary dictionary = (IDictionary)obj;
			
			writer.Write(dictionary.Count);
			foreach (DictionaryEntry item in dictionary)
			{
				Serializer.WriteObjectRef(_typeKey, item.Key, writer);
				Serializer.WriteObjectRef(_typeValue, item.Value, writer);
			}
		}

		public override void LoadObjectData(object obj)
		{
			IDictionary iCollection = (IDictionary)obj;
			int count = Reader.ReadInt32();

			for (int j = 0; j < count; j++)
			{
				object key = _list1TypeRepo.LoadObjectRef();
				object value = _list2TypeRepo.LoadObjectRef();

				if (key != null)
					_addMethod.Invoke(iCollection, new object[] { key, value });
			}
		}

		public override void Clone(object source, object dest)
		{
			IDictionary iSource = (IDictionary)source;
			IDictionary iDest = (IDictionary)dest;
			foreach (DictionaryEntry item in iSource)
			{
				object key = Serializer.Clone(item.Key);
				object value = Serializer.Clone(item.Value);
				_addMethod.Invoke(iDest, new object[] { key, value });
			}
		}
	}
}
