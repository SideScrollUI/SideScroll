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
	/// <remarks>
	/// A value below one skips the save loop entirely, which would report success without writing
	/// </remarks>
	public static int SaveAttemptsMax
	{
		get => _saveAttemptsMax;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(SaveAttemptsMax));
			_saveAttemptsMax = value;
		}
	}
	private static int _saveAttemptsMax = 10;

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

		for (int attempt = 1; attempt <= SaveAttemptsMax; attempt++)
		{
			if (attempt > 1)
			{
				Thread.Sleep((attempt - 1) * SaveAttemptsBackoff);
			}

			// Serialize to a temp file and move it into place once it succeeds. Writing the
			// destination directly truncated the previous save before serializing anything, so a
			// failure destroyed it and left nothing to fall back on
			string tempPath = DataPath + "." + System.IO.Path.GetRandomFileName();
			bool moved = false;
			try
			{
				// FileShare.None avoids simultaneous writes
				using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					JsonSerializer.Serialize(stream, obj, obj.GetType(), options);
				}

				File.Move(tempPath, DataPath!, true);
				moved = true;

				string headerJson = JsonSerializer.Serialize(new JsonHeader { Name = name });
				File.WriteAllText(HeaderPath!, headerJson);
				return;
			}
			// The last attempt isn't caught, so the failure reaches the caller instead of
			// returning as though the save had succeeded
			catch (Exception e) when (attempt < SaveAttemptsMax)
			{
				call.Log.AddWarning("Save failed, retrying",
					new Tag("Path", DataPath),
					new Tag("Attempt", attempt),
					new Tag("Message", e.Message));
			}
			finally
			{
				// The move consumed it on success, only a failure leaves one behind
				if (!moved)
				{
					DeleteTempFile(tempPath);
				}
			}
		}
	}

	// A failed save shouldn't leave its temp file behind, and the delete can't hide the original error
	private static void DeleteTempFile(string tempPath)
	{
		try
		{
			File.Delete(tempPath);
		}
		catch (Exception)
		{
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
