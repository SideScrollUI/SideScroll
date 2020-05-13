﻿using Atlas.Core;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

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
			using (var writer = new BinaryWriter(stream, Encoding.Default, true))
			{
				var serializer = new Serializer();
				serializer.AddObject(call, obj);
				serializer.Save(call, writer);
			}
		}

		public T Load<T>(Call call = null)
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

		public object Load(Call call = null)
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
		public static T Clone<T>(Call call, object obj)
		{
			if (typeof(T) != obj.GetType())
			{
				throw new Exception("Cloned types do not match [" + typeof(T).ToString() + "], [" + obj.GetType().ToString() +"]");
			}
			//	return default;
			try
			{
				var memorySerializer = new SerializerMemory();
				memorySerializer.Save(call, obj);
				T copy = memorySerializer.Load<T>(call);
				return copy;
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
				var memorySerializer = new SerializerMemory();
				memorySerializer.Save(call, obj);
				object copy = memorySerializer.Load(call);
				return copy;
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}
			return null;
		}

		public string GetEncodedString()
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

		public void LoadEncodedString(string base64)
		{
			ConvertEncodedToStream(base64, stream);
		}
	}
}
