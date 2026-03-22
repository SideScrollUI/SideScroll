using System.Runtime.Versioning;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Demo.Avalonia.Browser;

/// <summary>
/// localStorage-based DataRepoView for browser applications
/// Inherits GetPathEnumerable override from DataRepoInstanceLocalStorage
/// </summary>
[SupportedOSPlatform("browser")]
public class DataRepoViewLocalStorage<T> : DataRepoView<T>
{
	public DataRepoViewLocalStorage(DataRepo dataRepo, string groupId, bool indexed = false, int? maxItems = null)
		: base(dataRepo, groupId, indexed, maxItems)
	{
	}

	/// <summary>
	/// Overrides GetPathEnumerable to scan localStorage keys instead of filesystem directories
	/// </summary>
	public override IEnumerable<string>? GetPathEnumerable(bool ascending)
	{
		Console.WriteLine($"🔵 DataRepoViewLocalStorage.GetPathEnumerable called: type={typeof(T).Name}, group={GroupId}");
		
		// Get the group path pattern to match localStorage keys
		string keyPrefix = SerializerLocalStorage.ConvertPathToStorageKey(GroupPath);
		
		Console.WriteLine($"🔍 Scanning localStorage with prefix: {keyPrefix}");

		// Get all localStorage keys that start with this prefix
		var allKeys = SerializerLocalStorage.GetAllKeys();
		var matchingKeys = allKeys.Where(k => k.StartsWith(keyPrefix + "_")).ToList();
		
		Console.WriteLine($"📦 Found {matchingKeys.Count} matching keys in localStorage");

		if (matchingKeys.Count == 0)
		{
			Console.WriteLine($"⚠️ No keys found - returning null");
			return null;
		}

		// Convert storage keys back to paths
		var paths = matchingKeys.Select(storageKey =>
		{
			// Remove prefix and convert back to path format
			// The storage key format is: SideScroll_Data__path_with_underscores
			// After removing "SideScroll_Data_": _path_with_underscores
			// Replace _ with /: /path/with/underscores (already has leading slash!)
			string path = storageKey.Substring("SideScroll_Data_".Length).Replace("_", "/");
			Console.WriteLine($"  ✅ Found path: {path}");
			return path; // Path already has leading slash from the conversion
		}).ToList();

		Console.WriteLine($"📊 Returning {paths.Count} paths");
		return ascending ? paths : paths.AsEnumerable().Reverse();
	}

	/// <summary>
	/// Overrides LoadAllDataItems to load from localStorage instead of using static DataRepo.LoadPath
	/// </summary>
	public override IEnumerable<DataItem<T>> LoadAllDataItems(Call call, bool ascending = true)
	{
		Console.WriteLine($"🔵 DataRepoViewLocalStorage.LoadAllDataItems called: type={typeof(T).Name}");
		
		var pathIterator = GetPathEnumerable(ascending);
		if (pathIterator == null)
		{
			Console.WriteLine($"⚠️ No paths found, returning empty");
			return [];
		}

		var items = new List<DataItem<T>>();
		foreach (string path in pathIterator)
		{
			try
			{
				Console.WriteLine($"🔄 Loading from path: {path}");
				
				// Create SerializerLocalStorage for this path
				var serializer = new SerializerLocalStorage(path);
				if (!serializer.Exists)
				{
					Console.WriteLine($"⚠️ Path doesn't exist in localStorage: {path}");
					continue;
				}
				
				T? obj = serializer.Load<T>(call, lazy: false);
				if (obj != null)
				{
					// Get the key from the object itself (e.g., planet name)
					string key = SideScroll.Utilities.ObjectUtils.GetObjectId(obj) ?? serializer.Name;
					Console.WriteLine($"✅ Loaded item: {key}");
					items.Add(new DataItem<T>(key, obj, path));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"❌ Error loading from {path}: {e.Message}");
			}
		}
		
		Console.WriteLine($"📊 LoadAllDataItems returning {items.Count} items");
		return items;
	}
}
