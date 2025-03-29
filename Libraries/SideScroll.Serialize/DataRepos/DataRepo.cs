using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Serialize.Atlas;
using System.Diagnostics;

namespace SideScroll.Serialize.DataRepos;

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

	public DataRepoInstance<T> Open<T>(string groupId, bool indexed = false)
	{
		return new DataRepoInstance<T>(this, groupId, indexed);
	}

	public DataRepoView<T> OpenView<T>(string groupId, bool indexed = false, int? maxItems = null)
	{
		return new DataRepoView<T>(this, groupId, indexed, maxItems);
	}

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

	public DataRepoView<T> LoadIndexedView<T>(Call call, string groupId, bool ascending = true)
	{
		var view = new DataRepoView<T>(this, groupId, true);
		view.LoadAllIndexed(call, ascending);
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

	public void Save<T>(string key, T obj, Call? call = null)
	{
		Save<T>(null, key, obj, call);
	}

	public void Save<T>(string? groupId, string key, T obj, Call? call = null)
	{
		Save(typeof(T), groupId, key, obj!, call);
	}

	public void Save(Type type, string? groupId, string key, object obj, Call? call = null)
	{
		groupId ??= DefaultGroupId;
		call ??= new();
		SerializerFile serializer = GetSerializerFile(type, groupId, key); // use hash since filesystems can't handle long names
		serializer.Save(call, obj, key);
	}

	public void Save<T>(T obj, Call? call = null)
	{
		Save(typeof(T).GetAssemblyQualifiedShortName(), obj, call);
	}

	public DataItem<T>? LoadItem<T>(string key, Call? call = null, bool createIfNeeded = false, bool lazy = false)
	{
		return LoadItem<T>(DefaultGroupId, key, call, createIfNeeded, lazy);
	}

	public DataItem<T>? LoadItem<T>(string groupId, string key, Call? call, bool createIfNeeded = false, bool lazy = false)
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

		if (createIfNeeded)
		{
			T newObject = Activator.CreateInstance<T>();
			Debug.Assert(newObject != null);
			return new DataItem<T>(key, newObject, serializerFile.DataPath);
		}
		return default;
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
		call ??= new();
		return Load<T>(typeof(T).GetAssemblyQualifiedShortName(), call, createIfNeeded, lazy);
	}

	public DataItem<T>? LoadPath<T>(Call? call, string path, bool lazy = false)
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

	public DataItemCollection<T> LoadAll<T>(Call? call = null, string? groupId = null, bool lazy = false)
	{
		call ??= new();
		groupId ??= DefaultGroupId;

		/*ItemCollection<string> objectIds = GetObjectIds(typeof(T));

		var list = new ItemCollection<T>();
		foreach (string id in objectIds)
		{
			T item = Load<T>(id, log, createIfNeeded, lazy, taskInstance);
			if (item != null)
				list.Add(item);
		}*/
		DataItemCollection<T> entries = [];

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
					{
						entries.Add(serializerFile.LoadHeader(call).Name ?? "", obj);
					}
				}
			}
		}
		return entries;
	}

	public List<Header> LoadHeaders(Type type, string? groupId = null, Call? call = null)
	{
		call ??= new();
		groupId ??= DefaultGroupId;

		List<Header> headers = [];

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
					{
						headers.Add(header);
					}
				}
			}
		}
		return headers;
	}

	public void DeleteAll<T>(Call? call, string? groupId = null)
	{
		DeleteAll(call, typeof(T), groupId);
	}

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

	// remove all other deletes and add null defaults?
	public void Delete<T>(Call? call, string groupId, string key)
	{
		Delete(call, typeof(T), groupId, key);
	}

	public void Delete<T>(Call? call, string key)
	{
		Delete(call, typeof(T), null, key);
	}

	public void Delete(Call? call, Type type, string key)
	{
		Delete(call, type, null, key);
	}

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

	// Don't use GetHashCode(), it returns a different value each time the process is run
	public string GetGroupPath(Type type, string? groupId = null)
	{
		groupId ??= DefaultGroupId;
		string groupHash = (type.GetNonNullableType().GetAssemblyQualifiedShortName() + ';' + RepoName + ';' + groupId).HashSha256();
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
