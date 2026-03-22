using System.Runtime.Versioning;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Demo.Avalonia.Browser;

/// <summary>
/// localStorage-based DataRepoInstance for browser applications
/// Overrides directory enumeration to scan localStorage instead
/// </summary>
[SupportedOSPlatform("browser")]
public class DataRepoInstanceLocalStorage<T> : DataRepoInstance<T>
{
	public DataRepoInstanceLocalStorage(DataRepo dataRepo, string groupId, bool indexed = false, int? maxItems = null) 
		: base(dataRepo, groupId, indexed, maxItems)
	{
	}

	/// <summary>
	/// Overrides GetPathEnumerable to scan localStorage keys instead of filesystem directories
	/// </summary>
	public override IEnumerable<string>? GetPathEnumerable(bool ascending)
	{
		Console.WriteLine($"🔵 DataRepoInstanceLocalStorage.GetPathEnumerable called: type={typeof(T).Name}, group={GroupId}");
		
		// Get the group path pattern to match localStorage keys
		string keyPrefix = SerializerLocalStorage.ConvertPathToStorageKey(GroupPath);
		
		Console.WriteLine($"🔍 Scanning localStorage with prefix: {keyPrefix}");

		// Get all localStorage keys that start with this prefix
		var allKeys = SerializerLocalStorage.GetAllKeys();
		var matchingKeys = allKeys.Where(k => k.StartsWith(keyPrefix + "_")).ToList();
		
		Console.WriteLine($"📦 Found {matchingKeys.Count} matching keys in localStorage");

		if (matchingKeys.Count == 0)
			return null;

		// Convert storage keys back to paths
		var paths = matchingKeys.Select(storageKey =>
		{
			// Remove prefix and convert back to path format
			string path = storageKey.Substring("SideScroll_Data_".Length).Replace("_", "/");
			Console.WriteLine($"  ✅ Found path: {path}");
			return "/" + path; // Add leading slash to match expected format
		});

		return ascending ? paths : paths.Reverse();
	}
}
