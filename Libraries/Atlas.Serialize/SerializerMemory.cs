using Atlas.Core;
using Atlas.Extensions;
using System.IO.Compression;

namespace Atlas.Serialize;

public abstract class SerializerMemory
{
	protected MemoryStream Stream { get; set; } = new(); // move to atlas class?
	public bool PublicOnly { get; set; } // Whether to save classes with the [PublicData] attribute

	public SerializerMemory() { }

	public abstract void Save(Call call, object obj);

	public abstract bool TryLoad<T>(out T? obj, Call? call = null) where T : class;

	public abstract T Load<T>(Call? call = null);

	public abstract object? Load(Call? call = null);

	// Save an object to a memory stream and then load it
	public static T? DeepClone<T>(Call call, T? obj, bool publicOnly = false) where T : class
	{
		if (obj == null) return null;

		//	return default;
		try
		{
			var memorySerializer = Create();
			memorySerializer.PublicOnly = publicOnly;
			using CallTimer timer = call.Timer(LogLevel.Debug, "Deep Cloning", new Tag("Object", obj.Formatted()));
			return memorySerializer.DeepCloneInternal(timer, obj);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
		return default;
	}

	public static object? DeepClone(Call call, object? obj, bool publicOnly = false)
	{
		if (obj == null) return null;

		try
		{
			var memorySerializer = Create();
			memorySerializer.PublicOnly = publicOnly;
			return memorySerializer.DeepCloneInternal(call, obj);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
		return null;
	}

	protected abstract T? DeepCloneInternal<T>(Call call, T obj) where T : class;

	protected abstract object? DeepCloneInternal(Call call, object obj);

	public string ToBase64String(Call call)
	{
		Stream.Seek(0, SeekOrigin.Begin);

		using var outStream = new MemoryStream();

		// The GZip stream must be disposed before calling outStream
		using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
		{
			Stream.CopyTo(tinyStream);
		}

		byte[] compressed = outStream.ToArray();
		string base64 = Convert.ToBase64String(compressed);
		call.Log.Add("ToBase64String",
			new Tag("Original", Stream.Length),
			new Tag("Compressed", compressed.Length),
			new Tag("Base64", base64.Length));
		return base64;
	}

	public static void ConvertEncodedToStream(string base64, Stream outStream)
	{
		byte[] bytes = Convert.FromBase64String(base64);
		using var inStream = new MemoryStream(bytes);
		using var tinyStream = new GZipStream(inStream, CompressionMode.Decompress);

		tinyStream.CopyTo(outStream);
	}

	public void LoadBase64String(string base64)
	{
		ConvertEncodedToStream(base64, Stream);
	}

	public static string? ToBase64String(Call call, object obj, bool publicOnly = false)
	{
		call ??= new Call();

		var serializer = Create();
		serializer.PublicOnly = publicOnly;
		serializer.Save(call, obj);
		string data = serializer.ToBase64String(call);
		return data;
	}

	public static SerializerMemory Create()
	{
		return new SerializerMemoryAtlas();
		// todo: Add SerializerMemoryJson
	}
}
