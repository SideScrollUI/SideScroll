using SideScroll.Serialize.KeyValue;
using System.Runtime.Versioning;

namespace SideScroll.Serialize.Browser;

/// <summary>
/// localStorage-based serializer implementation for browser applications
/// Stores data in browser localStorage instead of file system
/// </summary>
/// <remarks>
/// The serialization itself lives in <see cref="SerializerKeyValueStore"/>, which is storage
/// agnostic and covered by tests. This binds it to localStorage and keeps the static helpers the
/// repository classes call
/// </remarks>
[SupportedOSPlatform("browser")]
public class SerializerLocalStorage : SerializerKeyValueStore
{
	/// <summary>
	/// Initializes a new instance of the SerializerLocalStorage class
	/// </summary>
	/// <param name="basePath">Logical path used to generate storage key</param>
	/// <param name="name">Name for this storage instance</param>
	public SerializerLocalStorage(string basePath, string name = "") :
		base(LocalStorageKeyValueStore.Default, basePath, name)
	{
	}

	/// <summary>
	/// Gets all localStorage keys with the SideScroll data prefix
	/// </summary>
	public static List<string> GetAllKeys()
	{
		try
		{
			return [.. LocalStorageKeyValueStore.Default.GetKeys(StorageKeys.DataPrefix)];
		}
		catch
		{
			return [];
		}
	}

	/// <summary>
	/// Converts a file path to a localStorage key
	/// </summary>
	public static string ConvertPathToStorageKey(string path) => StorageKeys.DataKey(path);

	/// <summary>
	/// Converts a localStorage data key back to a file path
	/// </summary>
	/// <exception cref="ArgumentException">The key isn't a data key</exception>
	public static string ConvertStorageKeyToPath(string storageKey) => StorageKeys.ToPath(storageKey);

	/// <summary>Returns whether a storage key represents item data directly within the given logical group path.</summary>
	public static bool IsDataKeyInGroup(string storageKey, string groupPath)
		=> StorageKeys.IsDataKeyInGroup(storageKey, groupPath);

	/// <summary>Returns whether data exists for a logical path.</summary>
	public static bool PathExists(string path)
		=> LocalStorageKeyValueStore.Default.Exists(StorageKeys.DataKey(path));

	/// <summary>
	/// Gets an item from localStorage (public static helper for index)
	/// </summary>
	public static string? GetItem(string key) => LocalStorageKeyValueStore.Default.Get(key);

	/// <summary>
	/// Sets an item in localStorage (public static helper for index)
	/// </summary>
	/// <returns>False if it couldn't be stored, localStorage has a limited quota</returns>
	public static bool SetItem(string key, string value) => LocalStorageKeyValueStore.Default.Set(key, value);

	/// <summary>
	/// Returns whether an item exists in localStorage (public static helper for index)
	/// </summary>
	/// <remarks>
	/// Throws if localStorage can't be reached, so callers removing entries based on this
	/// don't treat a failure as everything being missing
	/// </remarks>
	public static bool ItemExists(string key) => LocalStorageKeyValueStore.Default.Exists(key);

	/// <summary>
	/// Removes an item from localStorage (public static helper for delete)
	/// </summary>
	public static void RemoveItem(string key) => LocalStorageKeyValueStore.Default.Remove(key);

	/// <summary>Removes data and metadata stored for a logical path.</summary>
	public static void RemovePath(string path)
	{
		LocalStorageKeyValueStore.Default.Remove(StorageKeys.DataKey(path));
		LocalStorageKeyValueStore.Default.Remove(StorageKeys.HeaderKey(path));
	}
}
