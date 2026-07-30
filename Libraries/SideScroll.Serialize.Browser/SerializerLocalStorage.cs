using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;
using SideScroll.Serialize.Atlas;
using SideScroll.Serialize.DataRepos;
using SideScroll.Serialize.Json;
using SideScroll.Tasks;

namespace SideScroll.Serialize.Browser;

/// <summary>
/// localStorage-based serializer implementation for browser applications
/// Stores data in browser localStorage instead of file system
/// </summary>
[SupportedOSPlatform("browser")]
public partial class SerializerLocalStorage : SerializerFile
{
	private const string StoragePrefix = "SideScroll_Data_";
	private const string HeaderStoragePrefix = "SideScroll_Header_";

	private sealed class StorageHeader
	{
		public int Version { get; set; } = 1;
		public string? Name { get; set; }
	}

	/// <summary>
	/// Gets the localStorage key for this serializer instance
	/// </summary>
	public string StorageKey { get; }

	/// <summary>
	/// Initializes a new instance of the SerializerLocalStorage class
	/// </summary>
	/// <param name="basePath">Logical path used to generate storage key</param>
	/// <param name="name">Name for this storage instance</param>
	public SerializerLocalStorage(string basePath, string name = "") : base(basePath, name)
	{
		StorageKey = ConvertPathToStorageKey(basePath);
		DataPath = basePath; // Keep original path for compatibility
	}

	/// <summary>
	/// Override Exists to check localStorage instead of file system
	/// </summary>
	public override bool Exists
	{
		get
		{
			try
			{
				return ExistsInStorage(StorageKey);
			}
			catch
			{
				return false;
			}
		}
	}

	/// <summary>
	/// No-op: localStorage does not require directory creation
	/// </summary>
	protected override void EnsureStorageExists() { }

	/// <summary>Serializes <paramref name="obj"/> to JSON and writes it to localStorage under <see cref="StorageKey"/>, logging success or failure to <paramref name="call"/>.</summary>
	protected override void SaveInternal(Call call, object obj, string? name = null, bool publicOnly = false)
	{
		var options = publicOnly
			? JsonConverters.PublicSerializerOptions
			: JsonConverters.PrivateSerializerOptions;

		try
		{
			string json = JsonSerializer.Serialize(obj, obj.GetType(), options);
			bool success = SetLocalStorageItem(StorageKey, json);

			if (success)
			{
				string headerJson = JsonSerializer.Serialize(new StorageHeader { Name = name });
				success = SetLocalStorageItem(ConvertPathToHeaderStorageKey(BasePath), headerJson);
			}

			if (success)
			{
				call.Log.AddDebug("Saved to localStorage",
					new Tag("Name", name),
					new Tag("Key", StorageKey),
					new Tag("Size", json.Length));
			}
			else
			{
				call.Log.AddWarning("Failed to save to localStorage",
					new Tag("Name", name),
					new Tag("Type", obj.GetType()),
					new Tag("Key", StorageKey));
			}
		}
		catch (Exception e)
		{
			call.Log.Add(e,
				new Tag("Name", name),
				new Tag("Type", obj.GetType()),
				new Tag("Key", StorageKey));
		}
	}

	/// <summary>Reads JSON from localStorage under <see cref="StorageKey"/>, deserializes it to <paramref name="expectedType"/> (or a generic dictionary if unspecified), and returns the result.</summary>
	protected override object? LoadInternal(Call call, bool lazy, TaskInstance? taskInstance, bool publicOnly = false, Type? expectedType = null)
	{
		var options = publicOnly
			? JsonConverters.PublicSerializerOptions
			: JsonConverters.PrivateSerializerOptions;

		try
		{
			string? json = GetLocalStorageItem(StorageKey);

			if (string.IsNullOrEmpty(json))
			{
				call.Log.AddDebug("No data found in localStorage",
					new Tag("Key", StorageKey));
				return null;
			}

			call.Log.AddDebug("Loaded from localStorage",
				new Tag("Key", StorageKey),
				new Tag("Size", json.Length));

			// Use expectedType if provided, otherwise fallback to Dictionary
			object? obj = expectedType != null
				? JsonSerializer.Deserialize(json, expectedType, options)
				: JsonSerializer.Deserialize<Dictionary<string, object?>>(json, options);

			// Report progress instead of finishing, the caller owns the task and the work isn't done until here
			if (taskInstance != null)
			{
				taskInstance.Percent = 100;
			}

			return obj;
		}
		catch (Exception e)
		{
			call.Log.Add(e, new Tag("Key", StorageKey));
			return null;
		}
	}

	/// <inheritdoc/>
	public override SerializerHeader LoadHeader(Call call)
	{
		string? json = GetLocalStorageItem(ConvertPathToHeaderStorageKey(BasePath));
		if (string.IsNullOrEmpty(json))
		{
			return new SerializerHeader { Name = Name };
		}

		StorageHeader? header = JsonSerializer.Deserialize<StorageHeader>(json);
		return new SerializerHeader
		{
			Version = header?.Version is { } version ? checked((ushort)version) : null,
			Name = header?.Name,
		};
	}

	/// <summary>
	/// Gets all localStorage keys with the SideScroll data prefix
	/// </summary>
	public static List<string> GetAllKeys()
	{
		try
		{
			// Since JSImport doesn't support string[], get them via JSON
			string json = GetKeysJson(StoragePrefix);
			var keys = JsonSerializer.Deserialize<List<string>>(json);
			return keys ?? [];
		}
		catch
		{
			return [];
		}
	}

	/// <summary>
	/// Converts a file path to a localStorage key
	/// </summary>
	public static string ConvertPathToStorageKey(string path)
	{
		string normalizedPath = path.Replace('\\', '/');
		return StoragePrefix + Uri.EscapeDataString(normalizedPath);
	}

	private static string ConvertPathToHeaderStorageKey(string path)
	{
		string normalizedPath = path.Replace('\\', '/');
		return HeaderStoragePrefix + Uri.EscapeDataString(normalizedPath);
	}

	/// <summary>
	/// Converts a localStorage data key back to a file path
	/// </summary>
	/// <exception cref="ArgumentException">The key isn't a data key</exception>
	public static string ConvertStorageKeyToPath(string storageKey)
	{
		// The header prefix also starts with "SideScroll_", so blindly trimming the data prefix
		// off one would leave part of it in the path instead of failing
		if (!storageKey.StartsWith(StoragePrefix, StringComparison.Ordinal))
		{
			throw new ArgumentException($"Not a {StoragePrefix} key: {storageKey}", nameof(storageKey));
		}

		string encodedPath = storageKey[StoragePrefix.Length..];
		return Uri.UnescapeDataString(encodedPath);
	}

	/// <summary>Returns whether a storage key represents item data directly within the given logical group path.</summary>
	public static bool IsDataKeyInGroup(string storageKey, string groupPath)
	{
		string normalizedGroup = groupPath.Replace('\\', '/').TrimEnd('/');
		string path = ConvertStorageKeyToPath(storageKey).Replace('\\', '/');
		if (!path.StartsWith(normalizedGroup + '/', StringComparison.Ordinal))
			return false;

		string relativePath = path[(normalizedGroup.Length + 1)..];
		return !relativePath.Contains('/') &&
			!relativePath.Equals(DataRepo.PrimaryIndexFileName, StringComparison.Ordinal);
	}

	/// <summary>Returns whether data exists for a logical path.</summary>
	public static bool PathExists(string path)
	{
		return ExistsInStorage(ConvertPathToStorageKey(path));
	}

	/// <summary>
	/// Gets an item from localStorage (public static helper for index)
	/// </summary>
	public static string? GetItem(string key)
	{
		return GetLocalStorageItem(key);
	}

	/// <summary>
	/// Sets an item in localStorage (public static helper for index)
	/// </summary>
	/// <returns>False if it couldn't be stored, localStorage has a limited quota</returns>
	public static bool SetItem(string key, string value)
	{
		return SetLocalStorageItem(key, value);
	}

	/// <summary>
	/// Returns whether an item exists in localStorage (public static helper for index)
	/// </summary>
	/// <remarks>
	/// Throws if localStorage can't be reached, so callers removing entries based on this
	/// don't treat a failure as everything being missing
	/// </remarks>
	public static bool ItemExists(string key)
	{
		return ExistsInStorage(key);
	}

	/// <summary>
	/// Removes an item from localStorage (public static helper for delete)
	/// </summary>
	public static void RemoveItem(string key)
	{
		RemoveLocalStorageItem(key);
	}

	/// <summary>Removes data and metadata stored for a logical path.</summary>
	public static void RemovePath(string path)
	{
		string key = ConvertPathToStorageKey(path);
		RemoveLocalStorageItem(key);
		RemoveLocalStorageItem(ConvertPathToHeaderStorageKey(path));
	}

	// JavaScript interop methods - using globalThis.BrowserStorage
	// NOTE: Requires importing the package's localStorage.js module first:
	// await JSHost.ImportAsync("SideScroll.Serialize.Browser", "../_content/SideScroll.Serialize.Browser/localStorage.js");
	[JSImport("globalThis.BrowserStorage.load")]
	private static partial string? GetLocalStorageItem(string key);

	[JSImport("globalThis.BrowserStorage.save")]
	private static partial bool SetLocalStorageItem(string key, string value);

	[JSImport("globalThis.BrowserStorage.exists")]
	private static partial bool ExistsInStorage(string key);

	[JSImport("globalThis.BrowserStorage.remove")]
	private static partial void RemoveLocalStorageItem(string key);

	[JSImport("globalThis.BrowserStorage.getKeysJson")]
	private static partial string GetKeysJson(string prefix);
}
