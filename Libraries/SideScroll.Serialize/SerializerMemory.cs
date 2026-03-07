using SideScroll.Extensions;
using SideScroll.Logs;
using SideScroll.Serialize.Atlas;
using System.IO.Compression;

namespace SideScroll.Serialize;

/// <summary>
/// Base class for in-memory serialization operations
/// </summary>
public abstract class SerializerMemory
{
	/// <summary>
	/// Gets or sets the memory stream used for serialization
	/// </summary>
	public MemoryStream Stream { get; protected set; } = new();

	/// <summary>
	/// Gets or sets whether to serialize only classes with the [PublicData] attribute
	/// </summary>
	public bool PublicOnly { get; set; }

	/// <summary>
	/// Saves an object to the memory stream
	/// </summary>
	public abstract void Save(Call call, object obj);

	/// <summary>
	/// Attempts to load an object from the memory stream
	/// </summary>
	public abstract bool TryLoad<T>(out T? obj, Call? call = null) where T : class;

	/// <summary>
	/// Loads an object of the specified type from the memory stream
	/// </summary>
	public abstract T Load<T>(Call? call = null);

	/// <summary>
	/// Loads an object from the memory stream
	/// </summary>
	public abstract object? Load(Call? call = null);

	/// <summary>
	/// Validates the serialized data without fully loading the object
	/// </summary>
	public abstract void Validate(Call? call = null);

	/// <summary>
	/// Creates a deep clone of an object by serializing and deserializing it
	/// </summary>
	public static T DeepClone<T>(Call call, T obj, bool publicOnly = false) where T : class
	{
		var memorySerializer = Create();
		memorySerializer.PublicOnly = publicOnly;
		using CallTimer timer = call.Timer(LogLevel.Debug, "Deep Cloning", new Tag("Object", obj.Formatted()));
		return memorySerializer.DeepCloneInternal(timer, obj);
	}

	/// <summary>
	/// Attempts to create a deep clone of an object, returning null if the operation fails
	/// </summary>
	public static T? TryDeepClone<T>(Call call, T? obj, bool publicOnly = false) where T : class
	{
		if (obj == null) return null;

		try
		{
			return DeepClone(call, obj, publicOnly);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
		return null;
	}

	/// <summary>
	/// Attempts to create a deep clone of an object, returning null if the operation fails (non-generic version)
	/// </summary>
	public static object? TryDeepClone(Call call, object? obj, bool publicOnly = false)
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

	/// <summary>
	/// Validates a base64-encoded serialized object without fully loading it
	/// </summary>
	public static void ValidateBase64(Call call, string base64, bool publicOnly = false)
	{
		ArgumentNullException.ThrowIfNull(nameof(base64));

		var memorySerializer = Create();
		memorySerializer.PublicOnly = publicOnly;
		memorySerializer.LoadBase64String(base64);
		memorySerializer.Validate(call);
	}

	/// <summary>
	/// Internal implementation for deep cloning a typed object
	/// </summary>
	protected abstract T DeepCloneInternal<T>(Call call, T obj) where T : class;

	/// <summary>
	/// Internal implementation for deep cloning an object
	/// </summary>
	protected abstract object? DeepCloneInternal(Call call, object obj);

	/// <summary>
	/// Converts the memory stream to a compressed base64 string
	/// </summary>
	public string ToBase64String(Call call)
	{
		Stream.Seek(0, SeekOrigin.Begin);

		return ConvertStreamToBase64String(call, Stream);
	}

	/// <summary>
	/// Converts a memory stream to a compressed base64 string
	/// </summary>
	public static string ConvertStreamToBase64String(Call call, MemoryStream stream)
	{
		stream.Seek(0, SeekOrigin.Begin);

		using var outStream = new MemoryStream();

		// The GZip stream must be disposed before calling outStream
		using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
		{
			stream.CopyTo(tinyStream);
		}

		byte[] compressed = outStream.ToArray();
		string base64 = Convert.ToBase64String(compressed);
		call.Log.Add("ToBase64String",
			new Tag("Original", stream.Length),
			new Tag("Compressed", compressed.Length),
			new Tag("Base64", base64.Length));
		return base64;
	}

	/// <summary>
	/// Converts a compressed base64 string to a stream
	/// </summary>
	public static void ConvertEncodedToStream(string base64, Stream outStream)
	{
		byte[] bytes = Convert.FromBase64String(base64);
		using var inStream = new MemoryStream(bytes);
		using var tinyStream = new GZipStream(inStream, CompressionMode.Decompress);

		tinyStream.CopyTo(outStream);
	}

	/// <summary>
	/// Loads serialized data from a base64 string into the memory stream
	/// </summary>
	public void LoadBase64String(string base64)
	{
		ConvertEncodedToStream(base64, Stream);
	}

	/// <summary>
	/// Serializes an object to a compressed base64 string
	/// </summary>
	public static string ToBase64String(Call call, object obj, bool publicOnly = false)
	{
		var serializer = Create();
		serializer.PublicOnly = publicOnly;
		serializer.Save(call, obj);
		string base64 = serializer.ToBase64String(call);
		return base64;
	}

	/// <summary>
	/// Creates a new memory serializer instance
	/// </summary>
	public static SerializerMemory Create()
	{
		return new SerializerMemoryAtlas();
		// todo: Add SerializerMemoryJson
	}
}
