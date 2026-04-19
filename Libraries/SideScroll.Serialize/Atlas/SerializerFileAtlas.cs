using SideScroll.Tasks;

namespace SideScroll.Serialize.Atlas;

/// <summary>
/// File-based serializer implementation using the Atlas format
/// </summary>
public class SerializerFileAtlas : SerializerFile
{
	/// <summary>
	/// The default filename for Atlas data files
	/// </summary>
	public const string DataFileName = "Data.atlas";

	/// <summary>
	/// Gets or sets the maximum number of save attempts when file is locked
	/// </summary>
	public static int SaveAttemptsMax { get; set; } = 10;

	/// <summary>
	/// Gets or sets the backoff time between save attempts (multiplied by attempt number)
	/// </summary>
	public static TimeSpan SaveAttemptsBackoff { get; set; } = TimeSpan.FromMilliseconds(10);

	/// <summary>
	/// Initializes a new instance of the SerializerFileAtlas class
	/// </summary>
	public SerializerFileAtlas(string basePath, string name = "") : base(basePath, name)
	{
		HeaderPath = Paths.Combine(basePath, DataFileName);
		DataPath = Paths.Combine(basePath, DataFileName);
	}

	protected override void SaveInternal(Call call, object obj, string? name = null, bool publicOnly = false)
	{
		for (int attempt = 0; attempt < SaveAttemptsMax; attempt++)
		{
			if (attempt > 0)
			{
				Thread.Sleep(attempt * SaveAttemptsBackoff);
			}

			try
			{
				// Don't allow reading until finished since we seek backwards at the end to set the file size
				// FileShare.None also avoids simultaneous writes
				using var stream = new FileStream(DataPath!, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

				using var writer = new BinaryWriter(stream);

				Serializer serializer = new()
				{
					PublicOnly = publicOnly,
				};
				if (name != null)
				{
					serializer.Header.Name = name;
				}
				serializer.AddObject(call, obj);
				serializer.Save(call, writer);
				break;
			}
			catch (Exception e)
			{
				call.Log.Add(e.Message);
			}
		}
	}

	protected override object? LoadInternal(Call call, bool lazy, TaskInstance? taskInstance, bool publicOnly = false, Type? expectedType = null)
	{
		Serializer serializer = new()
		{
			PublicOnly = publicOnly,
		};

		MemoryStream memoryStream;
		using (CallTimer callReadAllBytes = call.Timer("Loading file",
			new Tag("Name", Name),
			new Tag("Path", DataPath)))
		{
			memoryStream = new MemoryStream(File.ReadAllBytes(DataPath!));
		}

		var reader = new BinaryReader(memoryStream);

		serializer.Load(call, reader, Name, true, lazy);
		object? obj;
		using (CallTimer callLoadBaseObject = call.Timer("Loading base object"))
		{
			obj = serializer.BaseObject(callLoadBaseObject);
		}
		serializer.LogLoadedTypes(call);

		if (taskInstance != null)
		{
			taskInstance.Percent = 100;
		}

		if (!lazy)
		{
			serializer.Dispose();
		}

		return obj;
	}

	/*public Header LoadHeader(Call call)
	{
		call ??= new();

		using (CallTimer callReadAllBytes = call.Timer("Loading header: " + Name))
		{
			var memoryStream = new MemoryStream(File.ReadAllBytes(FilePath));

			var reader = new BinaryReader(memoryStream);
			var header = new Header();
			header.Load(reader);
			return header;
		}
	}*/

	/// <summary>
	/// Loads the serializer schema without loading the object data
	/// </summary>
	public Serializer LoadSchema(Call call)
	{
		using var fileStream = new FileStream(HeaderPath!, FileMode.Open, FileAccess.Read, FileShare.Read);

		using var reader = new BinaryReader(fileStream);

		Serializer serializer = new();
		serializer.Load(call, reader, Name, false);
		return serializer;
	}

	/// <summary>
	/// Loads an existing object or creates a new instance if it doesn't exist
	/// </summary>
	public T LoadOrCreate<T>(Call? call = null, bool lazy = false, TaskInstance? taskInstance = null)
	{
		call ??= new();

		T? result = default;
		if (Exists)
		{
			result = Load<T>(call, lazy, taskInstance);
		}

		if (result != null) return result;

		T newObject = Activator.CreateInstance<T>();
		return newObject;
	}
}
