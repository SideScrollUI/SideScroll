using System.Runtime.Versioning;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Serialize.Browser;

/// <summary>
/// localStorage-based DataRepoInstance for browser applications
/// Overrides directory enumeration to scan localStorage instead
/// </summary>
[SupportedOSPlatform("browser")]
public class DataRepoInstanceLocalStorage<T> : DataRepoInstance<T>
{
	public DataRepoInstanceLocalStorage(DataRepo dataRepo, string groupId, bool indexed = false, int? maxItems = null) 
		: base(dataRepo, groupId, false, maxItems) // Don't let base class create index
	{
		// Create localStorage-compatible index if needed
		if (indexed)
		{
			Index = new DataRepoIndexLocalStorage<T>(this, maxItems);
		}
	}

	/// <summary>
	/// Overrides GetPathEnumerable to use index when available or scan localStorage
	/// </summary>
	public override IEnumerable<string>? GetPathEnumerable(Call call, bool ascending)
	{
		// If we have an index, use it for ordered access
		if (Index != null)
		{
			var indices = Index.Load(call);
			var paths = indices.Items.Select(i => DataRepo.GetDataPath(DataType, GroupId, i.Key));
			return ascending ? paths : paths.Reverse();
		}
		
		// Otherwise, scan localStorage keys
		string keyPrefix = SerializerLocalStorage.ConvertPathToStorageKey(GroupPath);
		var allKeys = SerializerLocalStorage.GetAllKeys();
		var matchingKeys = allKeys
			.Where(k => k.StartsWith(keyPrefix + '_'))
			.ToList();

		if (matchingKeys.Count == 0)
			return null;

		// Convert storage keys back to paths
		var scannedPaths = matchingKeys.Select(storageKey =>
		{
			string path = storageKey["SideScroll_Data_".Length..]
				.Replace('_', '/');
			return '/' + path; // Add leading slash to match expected format
		});

		return ascending ? scannedPaths : scannedPaths.Reverse();
	}
}
