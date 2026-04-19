using System.Runtime.Versioning;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Serialize.Browser;

/// <summary>
/// localStorage-based DataRepoView for browser applications
/// </summary>
[SupportedOSPlatform("browser")]
public class DataRepoViewLocalStorage<T> : DataRepoView<T>
{
	public DataRepoViewLocalStorage(DataRepo dataRepo, string groupId, bool indexed = false, int? maxItems = null)
		: base(dataRepo, groupId, false, maxItems) // Don't let base class create a file-system index
	{
		// Create localStorage-compatible index if needed
		if (indexed)
		{
			Index = new DataRepoIndexLocalStorage<T>(this, maxItems);
		}
	}

	/// <summary>
	/// Overrides GetPathEnumerable to use the index for ordered access when available,
	/// or to scan localStorage keys as a fallback.
	/// </summary>
	public override IEnumerable<string>? GetPathEnumerable(Call call, bool ascending)
	{
		// If we have an index, use it for deterministic ordered access
		if (Index != null)
		{
			var indices = Index.Load(call);
			var paths = indices.Items.Select(i => DataRepo.GetDataPath(DataType, GroupId, i.Key));
			return ascending ? paths : paths.Reverse();
		}

		// Fallback: scan localStorage keys for unindexed views
		string keyPrefix = SerializerLocalStorage.ConvertPathToStorageKey(GroupPath);
		var allKeys = SerializerLocalStorage.GetAllKeys();
		var matchingKeys = allKeys
			.Where(k => k.StartsWith(keyPrefix + "_"))
			.ToList();

		if (matchingKeys.Count == 0)
			return null;

		var scannedPaths = matchingKeys.Select(storageKey =>
		{
			string path = storageKey
				.Substring("SideScroll_Data_".Length)
				.Replace('_', '/');
			return path;
		}).ToList();

		return ascending ? scannedPaths : scannedPaths.AsEnumerable().Reverse();
	}

	/// <summary>
	/// Overrides LoadAllDataItems to load from localStorage instead of the file system.
	/// </summary>
	public override IEnumerable<DataItem<T>> LoadAllDataItems(Call call, bool ascending = true)
	{
		var pathIterator = GetPathEnumerable(call, ascending);
		if (pathIterator == null)
			return [];

		List<DataItem<T>> items = [];
		foreach (string path in pathIterator)
		{
			try
			{
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
