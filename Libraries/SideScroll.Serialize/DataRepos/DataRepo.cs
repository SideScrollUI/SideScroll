using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Serialize.Atlas;
using System.Diagnostics;

namespace SideScroll.Serialize.DataRepos;

/// <summary>
/// Manages a file-based data repository for storing and retrieving serialized objects
/// </summary>
[Unserialized]
public class DataRepo
{
	private const string DefaultGroupId = ".Default";

	/// <summary>
	/// Gets the root path of the repository
	/// </summary>
	public string RepoPath { get; }
	
	/// <summary>
	/// Gets the repository name used as an additional seed in the group hash
	/// </summary>
	public string? RepoName { get; }

	//public RepoSettings Settings;

	public override string ToString() => RepoPath;

	public DataRepo(string repoPath, string? repoName = null)
	{
		RepoPath = repoPath;
		RepoName = repoName;

		Debug.Assert(repoPath != null);
	}

	/// <summary>
	/// Opens a repository instance for the specified type and group without loading data
	/// </summary>
	public DataRepoInstance<T> Open<T>(string groupId, bool indexed = false)
	{
		return new DataRepoInstance<T>(this, groupId, indexed);
	}

	/// <summary>
	/// Opens a repository view for the specified type and group without loading data
	/// </summary>
	public DataRepoView<T> OpenView<T>(string groupId, bool indexed = false, int? maxItems = null)
	{
		return new DataRepoView<T>(this, groupId, indexed, maxItems);
	}

	/// <summary>
	/// Loads a repository view with all items, optionally ordered by a member name
	/// </summary>
	public DataRepoView<T> LoadView<T>(Call call, string groupId, string? orderByMemberName = null, bool ascending = true)
	{
		var view = new DataRepoView<T>(this, groupId);
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
	/// Loads an indexed repository view with all items
	/// </summary>
	public DataRepoView<T> LoadIndexedView<T>(Call call, string groupId, bool ascending = true)
	{
		var view = new DataRepoView<T>(this, groupId, true);
		view.LoadAllIndexed(call, ascending);
		return view;
	}

	/// <summary>
	/// Gets file information for the specified item
	/// </summary>
	public FileInfo GetFileInfo(Type type, string groupId, string key)
	{
		string dataPath = GetDataPath(type, groupId, key);
		return new FileInfo(dataPath);
	}

	/// <summary>
	/// Gets a serializer file for the specified type and key
	/// </summary>
	public SerializerFile GetSerializerFile(Type type, string key)
	{
		return GetSerializerFile(type, DefaultGroupId, key);
	}

	/// <summary>
	/// Gets a serializer file for the specified type, group, and key
	/// </summary>
	public SerializerFile GetSerializerFile(Type type, string groupId, string key)
	{
		string dataPath = GetDataPath(type, groupId, key);
		var serializer = SerializerFile.Create(dataPath, key);
		return serializer;
	}

	/// <summary>
	/// Saves an object with the specified key
	/// </summary>
	public void Save<T>(string key, T obj, Call? call = null)
	{
		Save<T>(null, key, obj, call);
	}

	/// <summary>
	/// Saves an object with the specified group and key
	/// </summary>
	public void Save<T>(string? groupId, string key, T obj, Call? call = null)
	{
		Save(typeof(T), groupId, key, obj!, call);
	}

	/// <summary>
	/// Saves an object of the specified type with the given group and key
	/// </summary>
	public void Save(Type type, string? groupId, string key, object obj, Call? call = null)
	{
		groupId ??= DefaultGroupId;
		call ??= new();
		SerializerFile serializer = GetSerializerFile(type, groupId, key); // use hash since filesystems can't handle long names
		serializer.Save(call, obj, key);
	}

	/// <summary>
	/// Saves an object using the type name as the key
	/// </summary>
	public void Save<T>(T obj, Call? call = null)
	{
		Save(typeof(T).GetAssemblyQualifiedShortName(), obj, call);
	}

	/// <summary>
	/// Loads a data item with the specified key
	/// </summary>
	public DataItem<T>? LoadItem<T>(string key, Call? call = null, bool lazy = false)
	{
		return LoadItem<T>(DefaultGroupId, key, call, lazy);
	}

	/// <summary>
	/// Loads a data item with the specified group and key
	/// </summary>
	public DataItem<T>? LoadItem<T>(string groupId, string key, Call? call, bool lazy = false)
	{
		SerializerFile serializerFile = GetSerializerFile(typeof(T), groupId, key);

		if (serializerFile.Exists)
		{
			T? obj = serializerFile.Load<T>(call, lazy);
			if (obj != null)
			{
				return new DataItem<T>(key, obj, serializerFile.DataPath);
			}
		}
		return null;
	}

	/// <summary>
	/// Loads a data item with the specified key, creating a new instance if it doesn't exist
	/// </summary>
	public DataItem<T> LoadOrCreateItem<T>(string key, Call? call = null, bool lazy = false)
	{
		return LoadOrCreateItem<T>(DefaultGroupId, key, call, lazy);
	}

	/// <summary>
	/// Loads a data item with the specified group and key, creating a new instance if it doesn't exist
	/// </summary>
	public DataItem<T> LoadOrCreateItem<T>(string groupId, string key, Call? call, bool lazy = false)
	{
		SerializerFile serializerFile = GetSerializerFile(typeof(T), groupId, key);

		if (serializerFile.Exists)
		{
			T? obj = serializerFile.Load<T>(call, lazy);
			if (obj != null)
			{
				return new DataItem<T>(key, obj, serializerFile.DataPath);
			}
		}

		T newObject = Activator.CreateInstance<T>();
		Debug.Assert(newObject != null);
		return new DataItem<T>(key, newObject, serializerFile.DataPath);
	}

	/// <summary>
	/// Loads an object with the specified key
	/// </summary>
	public T? Load<T>(string key, Call? call = null, bool lazy = false)
	{
		return Load<T>(DefaultGroupId, key, call, lazy);
	}

	/// <summary>
	/// Loads an object with the specified group and key
	/// </summary>
	public T? Load<T>(string groupId, string key, Call? call, bool lazy = false)
	{
		SerializerFile serializerFile = GetSerializerFile(typeof(T), groupId, key);

		if (serializerFile.Exists)
		{
			T? obj = serializerFile.Load<T>(call, lazy);
			if (obj != null)
				return obj;
		}

		return default;
	}

	/// <summary>
	/// Loads an object using the type name as the key
	/// </summary>
	public T? Load<T>(bool lazy = false, Call? call = null)
	{
		call ??= new();
		return Load<T>(typeof(T).GetAssemblyQualifiedShortName(), call, lazy);
	}

	/// <summary>
	/// Loads an object with the specified key, creating a new instance if it doesn't exist
	/// </summary>
	public T LoadOrCreate<T>(string key, Call? call = null, bool lazy = false)
	{
		return LoadOrCreate<T>(DefaultGroupId, key, call, lazy);
	}

	/// <summary>
	/// Loads an object with the specified group and key, creating a new instance if it doesn't exist
	/// </summary>
	public T LoadOrCreate<T>(string groupId, string key, Call? call, bool lazy = false)
	{
		SerializerFile serializerFile = GetSerializerFile(typeof(T), groupId, key);

		if (serializerFile.Exists)
		{
			T? obj = serializerFile.Load<T>(call, lazy);
			if (obj != null)
				return obj;
		}

		T newObject = Activator.CreateInstance<T>();
		Debug.Assert(newObject != null);
		return newObject;
	}

	/// <summary>
	/// Loads an object using the type name as the key, creating a new instance if it doesn't exist
	/// </summary>
	public T LoadOrCreate<T>(bool lazy = false, Call? call = null)
	{
		call ??= new();
		return LoadOrCreate<T>(typeof(T).GetAssemblyQualifiedShortName(), call, lazy);
	}

	/// <summary>
	/// Loads a data item from the specified file path
	/// </summary>
	public static DataItem<T>? LoadPath<T>(Call? call, string path, bool lazy = false)
	{
		call ??= new();

		var serializerFile = SerializerFile.Create(path);
		if (serializerFile.Exists)
		{
			T? obj = serializerFile.Load<T>(call, lazy);
			if (obj != null)
			{
				return new DataItem<T>(serializerFile.LoadHeader(call).Name ?? "", obj);
			}
		}
		return null;
	}

	/// <summary>
	/// Loads all items from the repository
	/// </summary>
	public DataItemCollection<T> LoadAll<T>(Call? call = null, string? groupId = null, bool lazy = false)
	{
		call ??= new();
		groupId ??= DefaultGroupId;

		DataItemCollection<T> entries = [];

		string groupPath = GetGroupPath(typeof(T), groupId);
		if (Directory.Exists(groupPath))
		{
			foreach (string filePath in Directory.EnumerateDirectories(groupPath))
			{
				var serializerFile = SerializerFile.Create(filePath);
				if (!serializerFile.Exists) continue;
				
				T? obj = serializerFile.Load<T>(call, lazy);
				if (obj != null)
				{
					entries.Add(serializerFile.LoadHeader(call).Name ?? "", obj);
				}
			}
		}
		return entries;
	}

	/// <summary>
	/// Loads all serializer headers for the specified type and group
	/// </summary>
	public List<SerializerHeader> LoadHeaders(Type type, string? groupId = null, Call? call = null)
	{
		call ??= new();
		groupId ??= DefaultGroupId;

		List<SerializerHeader> headers = [];

		string groupPath = GetGroupPath(type, groupId);
		if (Directory.Exists(groupPath))
		{
			foreach (string filePath in Directory.EnumerateDirectories(groupPath))
			{
				var serializerFile = SerializerFile.Create(filePath);
				if (!serializerFile.Exists) continue;
				
				SerializerHeader header = serializerFile.LoadHeader(call);
				headers.Add(header);
			}
		}
		return headers;
	}

	/// <summary>
	/// Deletes all items in the specified group
	/// </summary>
	public void DeleteAll<T>(Call? call, string? groupId = null)
	{
		DeleteAll(call, typeof(T), groupId);
	}

	/// <summary>
	/// Deletes all items of the specified type in the given group
	/// </summary>
	public void DeleteAll(Call? call, Type type, string? groupId = null)
	{
		call ??= new();

		string groupPath = GetGroupPath(type, groupId);
		if (!Directory.Exists(groupPath))
			return;

		try
		{
			Directory.Delete(groupPath, true);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
	}

	/// <summary>
	/// Deletes an item with the specified group and key
	/// </summary>
	public void Delete<T>(Call? call, string groupId, string key)
	{
		Delete(call, typeof(T), groupId, key);
	}

	/// <summary>
	/// Deletes an item with the specified key
	/// </summary>
	public void Delete<T>(Call? call, string key)
	{
		Delete(call, typeof(T), null, key);
	}

	/// <summary>
	/// Deletes an item of the specified type with the given key
	/// </summary>
	public void Delete(Call? call, Type type, string key)
	{
		Delete(call, type, null, key);
	}

	/// <summary>
	/// Deletes an item of the specified type with the given group and key
	/// </summary>
	public void Delete(Call? call, Type type, string? groupId, string key)
	{
		call ??= new();
		groupId ??= DefaultGroupId;

		string dataPath = GetDataPath(type, groupId, key);
		if (!Directory.Exists(dataPath))
			return;

		try
		{
			Directory.Delete(dataPath, true);
			call.Log.Add("Deleted path", new Tag("Path", dataPath));
		}
		catch (Exception e)
		{
			call.Log.Add(e, new Tag("Path", dataPath));
		}
	}

	/// <summary>
	/// Removes cached items older than the specified age
	/// </summary>
	public void CleanupCache(Call call, TimeSpan maxAge)
	{
		if (!Directory.Exists(RepoPath))
			return;

		DateTime threshold = DateTime.UtcNow - maxAge;

		foreach (string groupDirectory in Directory.EnumerateDirectories(RepoPath))
		{
			foreach (string dataDirectory in Directory.EnumerateDirectories(groupDirectory))
			{
				string filePath = Path.Combine(dataDirectory, SerializerFileAtlas.DataFileName);

				try
				{
					DateTime time = File.GetLastWriteTimeUtc(filePath); // or LastAccessTimeUtc
					if (time < threshold)
					{
						Directory.Delete(dataDirectory, true);
					}
				}
				catch (IOException) { /* Skip file if in use */ }
				catch (UnauthorizedAccessException) { /* Skip if no permission */ }
				catch (Exception ex)
				{
					call.Log.Add(ex, new Tag("Path", filePath));
				}
			}
		}
	}

	/// <summary>
	/// Deletes the entire repository directory
	/// </summary>
	public void DeleteRepo(Call? call = null)
	{
		call ??= new();

		if (!Directory.Exists(RepoPath))
		{
			call.Log.AddDebug("DataRepo has no directory to delete", new Tag("Path", RepoPath));
			return;
		}

		try
		{
			Directory.Delete(RepoPath, true);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
	}

	/// <summary>
	/// Generates a hash for the group identifier based on type and group ID
	/// </summary>
	public string GetGroupHash(Type type, string? groupId = null)
	{
		groupId ??= DefaultGroupId;

		// Don't use GetHashCode(), it returns a different value each time the process is run
		return (type.GetNonNullableType().GetAssemblyQualifiedShortName() + ';' + RepoName + ';' + groupId).HashSha256ToBase32();
	}

	/// <summary>
	/// Gets the file system path for the specified type and group
	/// </summary>
	public string GetGroupPath(Type type, string? groupId = null)
	{
		string groupHash = GetGroupHash(type, groupId);
		return Paths.Combine(RepoPath, groupHash);
	}

	/// <summary>
	/// Gets the file system path for the specified type, group, and key
	/// </summary>
	public string GetDataPath(Type type, string groupId, string key)
	{
		string groupPath = GetGroupPath(type, groupId);
		string nameHash = key.HashSha256ToBase32();
		return Paths.Combine(groupPath, nameHash);
	}
}
