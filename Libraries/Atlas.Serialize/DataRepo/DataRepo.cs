using Atlas.Core;
using Atlas.Extensions;
using System.Diagnostics;

namespace Atlas.Serialize;

[Unserialized]
public class DataRepo
{
	private const string DefaultGroupId = ".Default";

	public string RepoPath { get; set; }
	public string RepoName { get; set; }

	//public RepoSettings Settings;

	public override string ToString() => RepoPath;

	public DataRepo(string repoPath, string repoName)
	{
		RepoPath = repoPath;
		RepoName = repoName;

		Debug.Assert(repoPath != null);
	}

	public DataRepoInstance<T> Open<T>(string groupId)
	{
		return new DataRepoInstance<T>(this, groupId);
	}

	public DataRepoView<T> OpenView<T>(string groupId)
	{
		return new DataRepoView<T>(this, groupId);
	}

	public DataRepoView<T> LoadView<T>(Call call, string groupId)
	{
		var view = new DataRepoView<T>(this, groupId);
		view.LoadAll(call);
		return view;
	}

	public DataRepoView<T> LoadView<T>(Call call, string groupId, string orderByMemberName)
	{
		var view = new DataRepoView<T>(this, groupId);
		view.LoadAllOrderBy(call, orderByMemberName);
		return view;
	}

	public FileInfo GetFileInfo(Type type, string groupId, string key)
	{
		string dataPath = GetDataPath(type, groupId, key);
		return new FileInfo(dataPath);
	}

	public SerializerFile GetSerializerFile(Type type, string key)
	{
		return GetSerializerFile(type, DefaultGroupId, key);
	}

	public SerializerFile GetSerializerFile(Type type, string groupId, string key)
	{
		string dataPath = GetDataPath(type, groupId, key);
		var serializer = SerializerFile.Create(dataPath, key);
		return serializer;
	}

	// Use ToString()? for key?
	public void Save(string key, object obj, Call? call = null)
	{
		Save(null, key, obj, call);
	}

	public void Save<T>(string key, T obj, Call? call = null)
	{
		Save<T>(null, key, obj, call);
	}

	public void Save(string? groupId, string key, object obj, Call? call = null)
	{
		Save(obj.GetType(), groupId, key, obj, call);
	}

	public void Save<T>(string? groupId, string key, T obj, Call? call = null)
	{
		Save(typeof(T), groupId, key, obj!, call);
	}

	public void Save(Type type, string? groupId, string key, object obj, Call? call = null)
	{
		groupId ??= DefaultGroupId;
		call ??= new Call();
		SerializerFile serializer = GetSerializerFile(type, groupId, key); // use hash since filesystems can't handle long names
		serializer.Save(call, obj, key);
	}

	public void Save(object obj, Call? call = null)
	{
		Save(obj.GetType().AssemblyQualifiedName!, obj, call);
	}

	public T? Load<T>(string key, Call? call = null, bool createIfNeeded = false, bool lazy = false)
	{
		return Load<T>(DefaultGroupId, key, call, createIfNeeded, lazy);
	}

	public T? Load<T>(string groupId, string key, Call? call, bool createIfNeeded = false, bool lazy = false)
	{
		SerializerFile serializerFile = GetSerializerFile(typeof(T), groupId, key);

		if (serializerFile.Exists)
		{
			T? obj = serializerFile.Load<T>(call, lazy);
			if (obj != null)
				return obj;
		}

		if (createIfNeeded)
		{
			T newObject = Activator.CreateInstance<T>();
			Debug.Assert(newObject != null);
			return newObject;
		}
		return default;
	}

	public T? Load<T>(bool createIfNeeded = false, bool lazy = false, Call? call = null)
	{
		call ??= new Call();
		return Load<T>(typeof(T).AssemblyQualifiedName!, call, createIfNeeded, lazy);
	}

	public DataItemCollection<T> LoadAll<T>(Call? call = null, string? groupId = null, bool lazy = false)
	{
		call ??= new Call();
		groupId ??= DefaultGroupId;

		/*ItemCollection<string> objectIds = GetObjectIds(typeof(T));

		var list = new ItemCollection<T>();
		foreach (string id in objectIds)
		{
			T item = Load<T>(id, log, createIfNeeded, lazy, taskInstance);
			if (item != null)
				list.Add(item);
		}*/
		DataItemCollection<T> entries = new();

		string groupPath = GetGroupPath(typeof(T), groupId);
		if (Directory.Exists(groupPath))
		{
			foreach (string filePath in Directory.EnumerateDirectories(groupPath))
			{
				var serializerFile = SerializerFile.Create(filePath);
				if (serializerFile.Exists)
				{
					T? obj = serializerFile.Load<T>(call, lazy);
					if (obj != null)
						entries.Add(serializerFile.LoadHeader(call).Name, obj);
				}
			}
		}
		return entries;
	}

	public SortedDictionary<string, T> LoadAllSorted<T>(Call? call = null, string? groupId = null, bool lazy = false)
	{
		return LoadAll<T>(call, groupId, lazy).Lookup;
	}

	public ItemCollection<Header> LoadHeaders(Type type, string? groupId = null, Call? call = null)
	{
		call ??= new Call();
		groupId ??= DefaultGroupId;

		ItemCollection<Header> headers = new();

		string groupPath = GetGroupPath(type, groupId);
		if (Directory.Exists(groupPath))
		{
			foreach (string filePath in Directory.EnumerateDirectories(groupPath))
			{
				var serializerFile = SerializerFile.Create(filePath);
				if (serializerFile.Exists)
				{
					Header header = serializerFile.LoadHeader(call);
					if (header != null)
						headers.Add(header);
				}
			}
		}
		return headers;
	}

	public void DeleteAll<T>(string? groupId = null)
	{
		DeleteAll(typeof(T), groupId);
	}

	public void DeleteAll(Type type, string? groupId = null)
	{
		string groupPath = GetGroupPath(type, groupId);
		if (!Directory.Exists(groupPath))
			return;

		try
		{
			Directory.Delete(groupPath, true);
		}
		catch (Exception)
		{
		}
	}

	// remove all other deletes and add null defaults?
	public void Delete<T>(string groupId, string key)
	{
		Delete(typeof(T), groupId, key);
	}

	public void Delete<T>(string key)
	{
		Delete(typeof(T), null, key);
	}

	public void Delete(Type type, string key)
	{
		Delete(type, null, key);
	}

	public void Delete(Type type, string? groupId, string key)
	{
		groupId ??= DefaultGroupId;
		string dataPath = GetDataPath(type, groupId, key);
		if (!Directory.Exists(dataPath))
			return;

		try
		{
			Directory.Delete(dataPath, true);
		}
		catch (Exception)
		{
		}
	}

	public void DeleteRepo()
	{
		if (!Directory.Exists(RepoPath))
			return;

		try
		{
			Directory.Delete(RepoPath, true);
		}
		catch (Exception)
		{
		}
	}

	// Don't use GetHashCode(), it returns a different value each time the process is run
	public string GetGroupPath(Type type, string? groupId = null)
	{
		groupId ??= DefaultGroupId;
		string groupHash = (type.GetNonNullableType().FullName + ';' + RepoName + ';' + groupId).HashSha256();
		return Paths.Combine(RepoPath, groupHash);
	}

	public string GetDataPath(Type type, string groupId, string key)
	{
		string groupPath = GetGroupPath(type, groupId);
		string nameHash = key.HashSha256();
		return Paths.Combine(groupPath, nameHash);
	}

	// clean this up?
	/*private ItemCollection<string> GetObjectIds(Type type, string key = null)
	{
		var list = new ItemCollection<string>();

		string typePath = GetTypePath(type, key);
		if (Directory.Exists(typePath))
		{
			foreach (string filePath in Directory.EnumerateDirectories(typePath))
			{
				string fileName = Path.GetFileName(filePath);
				string dataPath = Paths.Combine(filePath, DataName);
				if (File.Exists(dataPath) == false)
					continue;
				list.Add(fileName);
			}
		}
		return list;
	}*/
}
