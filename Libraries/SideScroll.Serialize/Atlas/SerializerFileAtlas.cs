using SideScroll.Tasks;

namespace SideScroll.Serialize.Atlas;

public class SerializerFileAtlas : SerializerFile
{
	public const string DataFileName = "Data.atlas";

	public static int SaveAttemptsMax = 10;
	public static TimeSpan SaveAttemptsBackoff = TimeSpan.FromMilliseconds(10); // Backoff * attempt #

	public SerializerFileAtlas(string basePath, string name = "") : base(basePath, name)
	{
		HeaderPath = Paths.Combine(basePath, DataFileName);
		DataPath = Paths.Combine(basePath, DataFileName);
	}

	protected override void SaveInternal(Call call, object obj, string? name = null)
	{
		for (int attempt = 0; attempt < SaveAttemptsMax; attempt++)
		{
			if (attempt > 0)
			{
				Thread.Sleep(attempt * (int)SaveAttemptsBackoff.TotalMilliseconds);
			}

			try
			{
				using var stream = new FileStream(DataPath!, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

				using var writer = new BinaryWriter(stream);

				var serializer = new Serializer();
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

	protected override object? LoadInternal(Call call, bool lazy, TaskInstance? taskInstance)
	{
		var serializer = new Serializer
		{
			TaskInstance = taskInstance
		};

		MemoryStream memoryStream;
		using (CallTimer callReadAllBytes = call.Timer("Loading file: " + Name))
		{
			memoryStream = new MemoryStream(File.ReadAllBytes(DataPath!));
		}

		var reader = new BinaryReader(memoryStream);

		serializer.Load(call, reader, lazy);
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

	public Serializer LoadSchema(Call call)
	{
		using var fileStream = new FileStream(HeaderPath!, FileMode.Open, FileAccess.Read, FileShare.Read);

		using var reader = new BinaryReader(fileStream);

		var serializer = new Serializer();
		serializer.Load(call, reader, false, false);
		return serializer;
	}

	public T LoadOrCreate<T>(Call? call = null, bool lazy = false, TaskInstance? taskInstance = null)
	{
		call ??= new();

		T? result = default;
		if (Exists)
		{
			result = Load<T>(call, lazy, taskInstance);
		}

		if (result == null)
		{
			T newObject = Activator.CreateInstance<T>();
			return newObject;
		}
		return result;
	}
}
