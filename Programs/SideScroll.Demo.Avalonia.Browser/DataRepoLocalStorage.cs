using System.Runtime.Versioning;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Demo.Avalonia.Browser;

/// <summary>
/// localStorage-based DataRepo for browser applications
/// Overrides file system operations to use localStorage instead
/// </summary>
[SupportedOSPlatform("browser")]
public class DataRepoLocalStorage : DataRepo
{
	public DataRepoLocalStorage(string repoPath, string? repoName = null) 
		: base(repoPath, repoName, useJson: true) // Always use JSON for localStorage
	{
	}

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
		Console.WriteLine($"🔵 DataRepoLocalStorage.LoadView called: type={typeof(T).Name}, group={groupId}");
		
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
	/// Gets a serializer file that uses localStorage instead of file system
	/// </summary>
	public override SerializerFile GetSerializerFile(Type type, string groupId, string key)
	{
		Console.WriteLine($"🟢 DataRepoLocalStorage.GetSerializerFile called: type={type.Name}, group={groupId}, key={key}");
		
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

		Console.WriteLine($"🔵 DataRepoLocalStorage.LoadAll called: type={typeof(T).Name}, group={groupId}");
		
		DataItemCollection<T> entries = [];

		// Get the group path pattern to match localStorage keys
		string groupPath = GetGroupPath(typeof(T), groupId);
		string keyPrefix = SerializerLocalStorage.ConvertPathToStorageKey(groupPath);
		
		Console.WriteLine($"🔍 Searching localStorage with prefix: {keyPrefix}");

		// Get all localStorage keys that start with this prefix
		var allKeys = SerializerLocalStorage.GetAllKeys();
		var matchingKeys = allKeys.Where(k => k.StartsWith(keyPrefix + "_")).ToList();
		
		Console.WriteLine($"📦 Found {matchingKeys.Count} matching keys in localStorage");

		foreach (string storageKey in matchingKeys)
		{
			try
			{
				// Extract the path from the storage key
				string path = storageKey.Substring("SideScroll_Data_".Length).Replace("_", "/");
				
				var serializerFile = new SerializerLocalStorage(path);
				if (!serializerFile.Exists)
				{
					Console.WriteLine($"⚠️ Key exists but Exists=false: {storageKey}");
					continue;
				}
				
				T? obj = serializerFile.Load<T>(call, lazy);
				if (obj != null)
				{
					// Extract name from storage key (last segment)
					string name = serializerFile.Name;
					Console.WriteLine($"✅ Loaded item: {name}");
					entries.Add(name, obj);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"❌ Error loading from {storageKey}: {e.Message}");
			}
		}
		
		Console.WriteLine($"📊 Loaded {entries.Count} items total");
		return entries;
	}
}
