using Atlas.Core;
using System;
using System.IO;
using System.IO.Compression;

namespace Atlas.Serialize
{
	public abstract class SerializerMemory
	{
		protected MemoryStream stream = new MemoryStream(); // move to atlas class?
		public bool SaveSecure { get; set; } = true; // Whether to save classes with the [Secure] attribute

		public SerializerMemory()
		{
		}

		public abstract void Save(Call call, object obj);

		public abstract T Load<T>(Call call = null);

		public abstract object Load(Call call = null);

		//public static T Clone<T>(Call call, T obj)
		public static T Clone<T>(Call call, object obj)
		{
			if (typeof(T) != obj.GetType())
			{
				throw new Exception("Cloned types do not match [" + typeof(T).ToString() + "], [" + obj.GetType().ToString() +"]");
			}
			//	return default;
			try
			{
				var memorySerializer = Create();
				return memorySerializer.CloneInternal<T>(call, obj);
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}
			return default;
		}

		public static object Clone(Call call, object obj)
		{
			try
			{
				var memorySerializer = Create();
				return memorySerializer.CloneInternal(call, obj);
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}
			return null;
		}

		public abstract T CloneInternal<T>(Call call, object obj);

		public abstract object CloneInternal(Call call, object obj);

		public string ToBase64String()
		{
			stream.Seek(0, SeekOrigin.Begin);
			using (var outStream = new MemoryStream())
			{
				using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
					stream.CopyTo(tinyStream);

				byte[] compressed = outStream.ToArray();
				string base64 = Convert.ToBase64String(compressed);
				return base64;
			}
		}

		public static void ConvertEncodedToStream(string base64, Stream outStream)
		{
			byte[] bytes = Convert.FromBase64String(base64);
			using (var inStream = new MemoryStream(bytes))
			{
				using (var tinyStream = new GZipStream(inStream, CompressionMode.Decompress))
					tinyStream.CopyTo(outStream);
			}
		}

		public void LoadBase64String(string base64)
		{
			ConvertEncodedToStream(base64, stream);
		}

		public static string ToBase64String(Call call, object obj)
		{
			var serializer = Create();
			serializer.Save(call, obj);
			string data = serializer.ToBase64String();
			return data;
		}

		public static SerializerMemory Create()
		{
			return new SerializerMemoryAtlas();
			// todo: Add SerializerMemoryJson
		}
	}
}
