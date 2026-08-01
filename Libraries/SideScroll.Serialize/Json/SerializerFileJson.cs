using SideScroll.Serialize.Atlas;
using SideScroll.Tasks;
using System.Text.Json;

namespace SideScroll.Serialize.Json;

/// <summary>
/// File-based serializer implementation using JSON format
/// </summary>
public class SerializerFileJson : SerializerFile
{
	/// <summary>
	/// The default filename for JSON data files
	/// </summary>
	public const string DataFileName = "Data.json";

	/// <summary>
	/// The filename used to preserve the serialized object's name
	/// </summary>
	public const string HeaderFileName = "Header.json";

	private sealed class JsonHeader
	{
		public int Version { get; set; } = 1;
		public string? Name { get; set; }
	}

	/// <summary>
	/// Gets or sets the maximum number of save attempts when file is locked
	/// </summary>
	public static int SaveAttemptsMax { get; set; } = 10;

	/// <summary>
	/// Gets or sets the backoff time between save attempts (multiplied by attempt number)
	/// </summary>
	public static TimeSpan SaveAttemptsBackoff { get; set; } = TimeSpan.FromMilliseconds(10);

	/// <summary>
	/// Initializes a new instance of the SerializerFileJson class
	/// </summary>
	public SerializerFileJson(string basePath, string? name = null) : base(basePath, name)
	{
		HeaderPath = Paths.Combine(basePath, HeaderFileName);
		DataPath = Paths.Combine(basePath, DataFileName);
	}

	/// <summary>
	/// Returns a header with the name from this serializer instance.
	/// JSON files do not use a binary header file; the name comes from the constructor.
	/// </summary>
	public override SerializerHeader LoadHeader(Call call)
	{
		if (!File.Exists(HeaderPath))
		{
			return new SerializerHeader { Name = Name };
		}

		string json = File.ReadAllText(HeaderPath);
		JsonHeader? jsonHeader = JsonSerializer.Deserialize<JsonHeader>(json);
		return new SerializerHeader
		{
			Version = jsonHeader?.Version is { } version ? checked((ushort)version) : null,
			Name = jsonHeader?.Name,
		};
	}

	protected override void SaveInternal(Call call, object obj, string? name = null, bool publicOnly = false)
	{
		var options = publicOnly
			? JsonConverters.PublicSerializerOptions
			: JsonConverters.PrivateSerializerOptions;

		for (int attempt = 0; attempt < SaveAttemptsMax; attempt++)
		{
			if (attempt > 0)
			{
				Thread.Sleep(attempt * SaveAttemptsBackoff);
			}

			try
			{
				// FileShare.None avoids simultaneous writes
				using (var stream = new FileStream(DataPath!, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					JsonSerializer.Serialize(stream, obj, obj.GetType(), options);
				}
				string headerJson = JsonSerializer.Serialize(new JsonHeader { Name = name });
				File.WriteAllText(HeaderPath!, headerJson);
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
		var options = publicOnly
			? JsonConverters.PublicSerializerOptions
			: JsonConverters.PrivateSerializerOptions;

		using CallTimer callReadAllBytes = call.Timer("Loading JSON file",
			new Tag("ExpectedType", expectedType),
			new Tag("Name", Name),
			new Tag("Path", DataPath));

		byte[] jsonBytes = File.ReadAllBytes(DataPath!);

		taskInstance?.SetFinished();

		// Use expectedType if provided, otherwise fallback to Dictionary
		return expectedType != null
			? JsonSerializer.Deserialize(jsonBytes, expectedType, options)
			: JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonBytes, options);
	}
}
