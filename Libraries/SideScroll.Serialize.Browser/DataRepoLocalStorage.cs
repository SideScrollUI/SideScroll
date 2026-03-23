using System.Runtime.Versioning;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Serialize.Browser;

/// <summary>
/// localStorage-based DataRepo for browser applications
/// Overrides file system operations to use localStorage instead
/// </summary>
[SupportedOSPlatform("browser")]
public class DataRepoLocalStorage(string repoPath, string? repoName = null)
	: DataRepo(repoPath, repoName, useJson: true)
{
	// Always use JSON for localStorage

	/// <summary>
	/// Opens a repository instance that uses localStorage
	/// </summary>
	public new DataRepoInstance<T> Open<T>(string groupId, bool indexed = false)
	{
		return new DataRepoInstanceLocalStorage<T>(this, groupId, indexed);
	}

	/// <summary>
	/// Opens a repository view that uses localStorage
	/// </summary>
	public new DataRepoView<T> OpenView<T>(string groupId, bool indexed = false, int? maxItems = null)
	{
		return new DataRepoViewLocalStorage<T>(this, groupId, indexed, maxItems);
	}

	/// <summary>
	/// Loads a repository view with all items using localStorage
	/// </summary>
	public override DataRepoView<T> LoadView<T>(Call call, string groupId, string? orderByMemberName = null, bool ascending = true)
	{
		var view = new DataRepoViewLocalStorage<T>(this, groupId);
		if (orderByMemberName != null)
		{
			view.LoadAllOrderBy(call, orderByMemberName, ascending);
		}
		else
		{
			view.LoadAll(call, ascending);
		}
		return view;
	}

	/// <summary>
	/// Loads an indexed repository view with all items using localStorage
	/// </summary>
	public override DataRepoView<T> LoadIndexedView<T>(Call call, string groupId, bool ascending = true)
	{
		var view = new DataRepoViewLocalStorage<T>(this, groupId, indexed: true);
		view.LoadAllIndexed(call, ascending);
		return view;
	}

	/// <summary>
	/// Gets a serializer file that uses localStorage instead of file system
	/// </summary>
	public override SerializerFile GetSerializerFile(Type type, string groupId, string key)
	{
		string dataPath = GetDataPath(type, groupId, key);
		return new SerializerLocalStorage(dataPath, key);
	}

	/// <summary>
	/// Overrides LoadAll to scan localStorage keys instead of filesystem directories
	/// </summary>
	public override DataItemCollection<T> LoadAll<T>(Call? call = null, string? groupId = null, bool lazy = false)
	{
		call ??= new();
		groupId ??= ".Default";

		DataItemCollection<T> entries = [];

		// Get the group path pattern to match localStorage keys
		string groupPath = GetGroupPath(typeof(T), groupId);
		string keyPrefix = SerializerLocalStorage.ConvertPathToStorageKey(groupPath);

		// Get all localStorage keys that start with this prefix
		var allKeys = SerializerLocalStorage.GetAllKeys();
		var matchingKeys = allKeys.Where(k => k.StartsWith(keyPrefix + "_")).ToList();

		foreach (string storageKey in matchingKeys)
		{
			try
			{
				// Extract the path from the storage key
				string path = storageKey.Substring("SideScroll_Data_".Length).Replace("_", "/");
				
				var serializerFile = new SerializerLocalStorage(path);
				if (!serializerFile.Exists)
					continue;
				
				T? obj = serializerFile.Load<T>(call, lazy);
				if (obj != null)
				{
					string name = serializerFile.Name;
					entries.Add(name, obj);
				}
			}
			catch (Exception e)
			{
				call.Log.Add(e, new Tag("Key", storageKey));
			}
		}
		
		return entries;
	}

	/// <summary>
	/// Deletes an item from localStorage
	/// </summary>
	public override void Delete(Call? call, Type type, string? groupId, string key)
	{
		call ??= new();
		groupId ??= ".Default";

		string dataPath = GetDataPath(type, groupId, key);
		string storageKey = SerializerLocalStorage.ConvertPathToStorageKey(dataPath);

		try
		{
			SerializerLocalStorage.RemoveItem(storageKey);
			call.Log.Add("Deleted from localStorage", new Tag("Key", storageKey));
		}
		catch (Exception e)
		{
			call.Log.Add(e, new Tag("Key", storageKey));
		}
	}

	/// <summary>
	/// Deletes all items in a group from localStorage
	/// </summary>
	public override void DeleteAll(Call? call, Type type, string? groupId = null)
	{
		call ??= new();
		groupId ??= ".Default";

		string groupPath = GetGroupPath(type, groupId);
		string keyPrefix = SerializerLocalStorage.ConvertPathToStorageKey(groupPath);

		// Get all localStorage keys that start with this prefix
		var allKeys = SerializerLocalStorage.GetAllKeys();
		var matchingKeys = allKeys.Where(k => k.StartsWith(keyPrefix + "_")).ToList();

		call.Log.Add("Deleting items from localStorage", new Tag("Type", type.Name), new Tag("Group", groupId), new Tag("Count", matchingKeys.Count));

		foreach (string storageKey in matchingKeys)
		{
			try
			{
				SerializerLocalStorage.RemoveItem(storageKey);
			}
			catch (Exception e)
			{
				call.Log.Add(e, new Tag("Key", storageKey));
			}
		}

		// Also delete the index if it exists
		string indexPath = Paths.Combine(groupPath, "Primary.sidx");
		string indexKey = SerializerLocalStorage.ConvertPathToStorageKey(indexPath);
		try
		{
			SerializerLocalStorage.RemoveItem(indexKey);
		}
		catch
		{
			// Index might not exist, that's okay
		}
	}
}
