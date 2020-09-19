using Atlas.Core;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Atlas.Serialize
{
	public class SerializerMemoryAtlas : SerializerMemory
	{
		//private MemoryStream stream = new MemoryStream();
		//public bool SaveSecure { get; set; } = true; // Whether to save classes with the [Secure] attribute

		public SerializerMemoryAtlas()
		{
		}

		public override void Save(Call call, object obj)
		{
			using (var writer = new BinaryWriter(stream, Encoding.Default, true))
			{
				var serializer = new Serializer()
				{
					SaveSecure = SaveSecure,
				};
				serializer.AddObject(call, obj);
				serializer.Save(call, writer);
			}
		}

		public override T Load<T>(Call call = null)
		{
			call = call ?? new Call();
			stream.Seek(0, SeekOrigin.Begin);
			using (var reader = new BinaryReader(stream))
			{
				var serializer = new Serializer();
				serializer.Load(call, reader);
				return (T)serializer.BaseObject();
			}
		}

		public override object Load(Call call = null)
		{
			call = call ?? new Call();
			stream.Seek(0, SeekOrigin.Begin);
			using (var reader = new BinaryReader(stream))
			{
				var serializer = new Serializer();
				serializer.Load(call, reader);
				return serializer.BaseObject();
			}
		}

		//public static T Clone<T>(Call call, T obj)
		public override T DeepCloneInternal<T>(Call call, object obj)
		{
			Save(call, obj);
			T copy = Load<T>(call);
			return copy;
		}

		public override object DeepCloneInternal(Call call, object obj)
		{
			Save(call, obj);
			object copy = Load(call);
			return copy;
		}
	}
}
