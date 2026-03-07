using SideScroll.Logs;
using SideScroll.Serialize.Atlas;
using SideScroll.Tasks;

namespace SideScroll.Serialize;

/// <summary>
/// Base class for file-based serialization operations
/// </summary>
public abstract class SerializerFile(string basePath, string name = "")
{
	private const string DefaultName = "<Default>";

	/// <summary>
	/// Gets the base path for the serializer files
	/// </summary>
	public string BasePath => basePath;
	
	/// <summary>
	/// Gets or sets the path to the header file
	/// </summary>
	public string? HeaderPath { get; set; }
	
	/// <summary>
	/// Gets or sets the path to the data file
	/// </summary>
	public string? DataPath { get; set; }
	
	/// <summary>
	/// Gets or sets the name of this serializer instance
	/// </summary>
	public string Name { get; set; } = name;

	/// <summary>
	/// Gets whether the data file exists and is non-empty
	/// </summary>
	public bool Exists => File.Exists(DataPath) && new FileInfo(DataPath).Length > 0;

	public override string ToString() => BasePath;

	/// <summary>
	/// Tests whether the data file is writable and has no open locks
	/// </summary>
	public void TestWrite()
	{
		File.WriteAllText(DataPath!, "");
	}

	/// <summary>
	/// Saves an object to the file system
	/// </summary>
	public void Save(Call call, object obj, string? name = null, bool publicOnly = false)
	{
		ArgumentNullException.ThrowIfNull(obj);

		name ??= DefaultName;

		using CallTimer callTimer = call.Timer(LogLevel.Debug, "Saving object", 
			new Tag("Name", name), 
			new Tag("Path", BasePath));

		if (!Directory.Exists(BasePath))
		{
			Directory.CreateDirectory(BasePath);
		}

		SaveInternal(callTimer, obj, name, publicOnly);
	}

	/// <summary>
	/// Internal implementation for saving an object
	/// </summary>
	protected abstract void SaveInternal(Call call, object obj, string? name = null, bool publicOnly = false);

	/// <summary>
	/// Loads an object of the specified type from the file
	/// </summary>
	public T? Load<T>(Call? call = null, bool lazy = false, TaskInstance? taskInstance = null)
	{
		call ??= new();
		object? obj = Load(call, lazy, taskInstance);
		if (obj is T loaded) return loaded;

		if (obj != null)
		{
			call.Log.Throw(new SerializerException("Loaded type doesn't match type specified",
				new Tag("Loaded Type", obj.GetType()),
				new Tag("Expected Type", typeof(T))));
		}

		return default;

		/*Type type = typeof(T);
		//if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
		{
			type = Nullable.GetUnderlyingType(type);
		}

		return (T)Convert.ChangeType(obj, type);*/
	}

	/// <summary>
	/// Loads an object from the file
	/// </summary>
	public object? Load(Call call, bool lazy = false, TaskInstance? taskInstance = null, LogLevel logLevel = LogLevel.Debug, bool publicOnly = false)
	{
		using CallTimer callTimer = call.Timer(logLevel, "Loading object", new Tag("Name", Name));

		try
		{
			return LoadInternal(callTimer, lazy, taskInstance, publicOnly);
		}
		catch (Exception e)
		{
			callTimer.Log.AddError("Exception loading file", new Tag("Exception", e.ToString()));
			return null; // returns null if reference type, otherwise default value (i.e. 0)
		}
	}

	/// <summary>
	/// Loads only the header from the file without loading the full object data
	/// </summary>
	public SerializerHeader LoadHeader(Call call)
	{
		using CallTimer callTimer = call.Timer(LogLevel.Debug, "Loading header", new Tag("Name", Name));

		var memoryStream = new MemoryStream(File.ReadAllBytes(HeaderPath!));

		var reader = new BinaryReader(memoryStream);
		var header = new SerializerHeader();
		header.Load(callTimer.Log, reader);
		return header;
	}

	/// <summary>
	/// Internal implementation for loading an object
	/// </summary>
	protected abstract object? LoadInternal(Call call, bool lazy, TaskInstance? taskInstance, bool publicOnly = false);

	/// <summary>
	/// Creates a serializer file instance for the specified path
	/// </summary>
	public static SerializerFile Create(string dataPath, string name = "")
	{
		return new SerializerFileAtlas(dataPath, name);
		// todo: Add SerializerFileJson
	}
}
