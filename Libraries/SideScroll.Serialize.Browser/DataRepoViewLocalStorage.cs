using System.Runtime.Versioning;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Serialize.Browser;

/// <summary>
/// localStorage-based DataRepoView for browser applications
/// Inherits GetPathEnumerable override from DataRepoInstanceLocalStorage
/// </summary>
[SupportedOSPlatform("browser")]
public class DataRepoViewLocalStorage<T>(DataRepo dataRepo, string groupId, bool indexed = false, int? maxItems = null)
	: DataRepoView<T>(dataRepo, groupId, indexed, maxItems)
{
	/// <summary>
	/// Overrides GetPathEnumerable to scan localStorage keys instead of filesystem directories
	/// </summary>
	public override IEnumerable<string>? GetPathEnumerable(bool ascending)
	{
		// Get the group path pattern to match localStorage keys
		string keyPrefix = SerializerLocalStorage.ConvertPathToStorageKey(GroupPath);

		// Get all localStorage keys that start with this prefix
		var allKeys = SerializerLocalStorage.GetAllKeys();
		var matchingKeys = allKeys.Where(k => k.StartsWith(keyPrefix + "_")).ToList();

		if (matchingKeys.Count == 0)
			return null;

		// Convert storage keys back to paths
		var paths = matchingKeys.Select(storageKey =>
		{
			string path = storageKey.Substring("SideScroll_Data_".Length).Replace("_", "/");
			return path; // Path already has leading slash from the conversion
		}).ToList();

		return ascending ? paths : paths.AsEnumerable().Reverse();
	}

	/// <summary>
	/// Overrides LoadAllDataItems to load from localStorage instead of using static DataRepo.LoadPath
	/// </summary>
	public override IEnumerable<DataItem<T>> LoadAllDataItems(Call call, bool ascending = true)
	{
		var pathIterator = GetPathEnumerable(ascending);
		if (pathIterator == null)
			return [];

		var items = new List<DataItem<T>>();
		foreach (string path in pathIterator)
		{
			try
			{
				// Create SerializerLocalStorage for this path
				var serializer = new SerializerLocalStorage(path);
				if (!serializer.Exists)
					continue;
				
				T? obj = serializer.Load<T>(call, lazy: false);
				if (obj == null) continue;
				string key = SideScroll.Utilities.ObjectUtils.GetObjectId(obj) ?? serializer.Name;
				items.Add(new DataItem<T>(key, obj, path));
			}
			catch (Exception e)
			{
				call.Log.Add(e, new Tag("Path", path));
			}
		}
		
		return items;
	}
}
