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
	/// <summary>Initializes the instance for the given group, optionally creating a localStorage-backed index.</summary>
	/// <param name="dataRepo">The owning data repository.</param>
	/// <param name="groupId">The group identifier for this instance's items.</param>
	/// <param name="indexed">Whether to maintain a localStorage-compatible index for ordered access.</param>
	/// <param name="maxItems">The maximum number of items to retain, or <c>null</c> for no limit.</param>
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
		var scannedPaths = matchingKeys.Select(SerializerLocalStorage.ConvertStorageKeyToPath);

		return ascending ? scannedPaths : scannedPaths.Reverse();
	}
}
