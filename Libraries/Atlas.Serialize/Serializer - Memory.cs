using Atlas.Core;
using System;
using System.IO;

namespace Atlas.Serialize
{
	public class SerializerMemory
	{
		private MemoryStream stream = new MemoryStream();

		public SerializerMemory()
		{
		}

		public void Save(Call call, object obj)
		{
			using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
			{
				Serializer serializer = new Serializer();
				serializer.AddObject(call, obj);
				serializer.Save(call, writer);
			}
		}

		public T Load<T>(Call call = null)
		{
			stream.Seek(0, SeekOrigin.Begin);
			using (BinaryReader reader = new BinaryReader(stream))
			{
				Serializer serializer = new Serializer();
				serializer.Load(call, reader);
				return (T)serializer.BaseObject();
			}
		}

		//public static T Clone<T>(Call call, T obj)
		public static T Clone<T>(Call call, object obj)
		{
			if (typeof(T) != obj.GetType())
				throw new Exception("Cloned types do not match");
			//	return default(T);
			SerializerMemory memorySerializer = new SerializerMemory();
			memorySerializer.Save(call, obj);
			T copy = memorySerializer.Load<T>(call);
			return copy;
		}
	}
}
