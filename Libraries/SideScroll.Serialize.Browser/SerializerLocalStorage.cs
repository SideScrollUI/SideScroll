using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;
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
				call.Log.AddDebug("Saved to localStorage",
					new Tag("Key", StorageKey),
					new Tag("Size", json.Length));
			}
			else
			{
				call.Log.AddWarning("Failed to save to localStorage",
					new Tag("Type", obj.GetType()),
					new Tag("Key", StorageKey));
			}
		}
		catch (Exception e)
		{
			call.Log.Add(e,
				new Tag("Type", obj.GetType()),
				new Tag("Key", StorageKey));
		}
	}

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
			
			taskInstance?.SetFinished();

			// Use expectedType if provided
			if (expectedType != null)
			{
				return JsonSerializer.Deserialize(json, expectedType, options);
			}

			// Fallback to Dictionary
			return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, options);
		}
		catch (Exception e)
		{
			call.Log.Add(e, new Tag("Key", StorageKey));
			return null;
		}
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
		string pathKey = path
			.Replace('\\', '_')
			.Replace('/', '_')
			.Replace(":", "");
		return StoragePrefix + pathKey;
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
	public static void SetItem(string key, string value)
	{
		SetLocalStorageItem(key, value);
	}

	/// <summary>
	/// Removes an item from localStorage (public static helper for delete)
	/// </summary>
	public static void RemoveItem(string key)
	{
		RemoveLocalStorageItem(key);
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
