using SideScroll.Logs;
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
	public SerializerFileJson(string basePath, string name = "") : base(basePath, name)
	{
		DataPath = Paths.Combine(basePath, DataFileName);
	}

	protected override void SaveInternal(Call call, object obj, string? name = null, bool publicOnly = false)
	{
		var options = publicOnly 
			? JsonConverters.PublicSerializerOptions 
			: JsonConverters.CreateOptions();

		for (int attempt = 0; attempt < SaveAttemptsMax; attempt++)
		{
			if (attempt > 0)
			{
				Thread.Sleep(attempt * SaveAttemptsBackoff);
			}

			try
			{
				// FileShare.None avoids simultaneous writes
				using var stream = new FileStream(DataPath!, FileMode.Create, FileAccess.Write, FileShare.None);
				JsonSerializer.Serialize(stream, obj, obj.GetType(), options);
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
			: JsonConverters.CreateOptions();

		using (CallTimer callReadAllBytes = call.Timer("Loading JSON file",
			new Tag("Name", Name),
			new Tag("Path", DataPath)))
		{
			byte[] jsonBytes = File.ReadAllBytes(DataPath!);
			
			taskInstance?.SetFinished();

			// Use expectedType if provided
			if (expectedType != null)
			{
				return JsonSerializer.Deserialize(jsonBytes, expectedType, options);
			}

			// Fallback to Dictionary
			return JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonBytes, options);
		}
	}

}
