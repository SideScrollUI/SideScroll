using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;
using SideScroll.Utilities;

namespace SideScroll.Serialize.DataRepos;

/// <summary>
/// Interface for data repository instances
/// </summary>
public interface IDataRepoInstance
{
	/// <summary>
	/// Gets the group identifier
	/// </summary>
	string GroupId { get; }
	
	/// <summary>
	/// Gets the group path
	/// </summary>
	string GroupPath { get; }

	/// <summary>
	/// Gets the data type stored in this repository
	/// </summary>
	Type DataType { get; }

	//object GetObject(string key);
}

/// <summary>
/// Manages a typed instance of a data repository group
/// </summary>
[Unserialized]
public class DataRepoInstance<T> : IDataRepoInstance
{
	/// <summary>
	/// Default key used when no key is specified
	/// </summary>
	protected const string DefaultKey = ".Default";

	/// <summary>
	/// Gets the parent data repository
	/// </summary>
	public DataRepo DataRepo { get; }

	/// <summary>
	/// Gets the group identifier
	/// </summary>
	public string GroupId { get; }
	
	/// <summary>
	/// Gets the hashed group identifier
	/// </summary>
	public string GroupHash => DataRepo.GetGroupHash(typeof(T), GroupId);
	
	/// <summary>
	/// Gets the file system path for this group
	/// </summary>
	public string GroupPath => DataRepo.GetGroupPath(typeof(T), GroupId);

	/// <summary>
	/// Gets the data type stored in this repository
	/// </summary>
	public Type DataType => typeof(T);

	/// <summary>
	/// Gets or sets the optional index for maintaining item order
	/// </summary>
	public DataRepoIndex<T>? Index { get; set; }

	public override string ToString() => GroupId;

	public DataRepoInstance(DataRepo dataRepo, string groupId, bool indexed = false, int? maxItems = null)
	{
		DataRepo = dataRepo;
		GroupId = groupId;
		if (indexed)
		{
			AddIndex(maxItems);
		}
	}

	/// <summary>
	/// Adds an index to maintain item order
	/// </summary>
	public void AddIndex(int? maxItems = null)
	{
		Index ??= new(this, maxItems);
	}

	/// <summary>
	/// Saves an item using the default key
	/// </summary>
	public virtual void Save(Call? call, T item)
	{
		Save(call, DefaultKey, item);
	}

	/// <summary>
	/// Saves multiple items
	/// </summary>
	public virtual void Save(Call? call, IEnumerable<T> items)
	{
		foreach (var item in items)
		{
			Save(call, item);
		}
	}

	/// <summary>
	/// Saves an item with the specified key
	/// </summary>
	public virtual void Save(Call? call, string key, T item)
	{
		call ??= new();
		Index?.Save(call, key);
		DataRepo.Save<T>(GroupId, key, item, call);
	}

	/// <summary>
	/// Loads an item with the specified key
	/// </summary>
	public virtual T? Load(Call? call, string? key = null, bool lazy = false)
	{
		return DataRepo.Load<T>(GroupId, key ?? DefaultKey, call, lazy);
	}

	/// <summary>
	/// Loads an item with the specified key or creates a new instance if it doesn't exist
	/// </summary>
	public virtual T LoadOrCreate(Call? call, string? key = null, bool lazy = false)
	{
		return DataRepo.LoadOrCreate<T>(GroupId, key ?? DefaultKey, call, lazy);
	}

	/// <summary>
	/// Creates a paged view for loading items
	/// </summary>
	public virtual DataPageView<T> LoadPageView(Call? call, bool ascending = true)
	{
		return new DataPageView<T>(this, ascending);
	}

	/// <summary>
	/// Loads all items from the repository
	/// </summary>
	public virtual DataItemCollection<T> LoadAll(Call? call = null, bool ascending = true)
	{
		call ??= new();
		return [.. LoadAllDataItems(call, ascending)];
	}

	/// <summary>
	/// Loads all serializer headers for the items in this group
	/// </summary>
	public List<SerializerHeader> LoadHeaders(Call? call = null)
	{
		return DataRepo.LoadHeaders(typeof(T), GroupId, call);
	}

	/// <summary>
	/// Deletes the specified item
	/// </summary>
	public virtual void Delete(Call? call, T item)
	{
		string key = ObjectUtils.GetObjectId(item)!;
		Delete(call, key);
	}

	/// <summary>
	/// Deletes multiple items
	/// </summary>
	public virtual void Delete(Call? call, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			Delete(call, item);
		}
	}

	/// <summary>
	/// Deletes an item with the specified key
	/// </summary>
	public virtual void Delete(Call? call = null, string? key = null)
	{
		call ??= new();
		key ??= DefaultKey;
		Index?.Remove(call, key);
		DataRepo.Delete<T>(call, GroupId, key);
	}

	/// <summary>
	/// Deletes all items from the repository
	/// </summary>
	public virtual void DeleteAll(Call? call)
	{
		call ??= new();
		Index?.RemoveAll(call);
		DataRepo.DeleteAll<T>(call, GroupId);
	}

	/// <summary>
	/// Gets an enumerable collection of file paths for all items
	/// </summary>
	public IEnumerable<string>? GetPathEnumerable(bool ascending)
	{
		if (!Directory.Exists(GroupPath)) return null;

		IEnumerable<string> enumerable;
		if (Index != null)
		{
			var indices = Index.Load(new());
			enumerable = indices.Items
				.Select(i => DataRepo.GetDataPath(DataType, GroupId, i.Key));
		}
		else
		{
			enumerable = Directory.EnumerateDirectories(GroupPath)
				.Select(path => path);
		}

		return ascending ? enumerable : enumerable.Reverse();
	}

	/// <summary>
	/// Loads all data items from the repository
	/// </summary>
	public IEnumerable<DataItem<T>> LoadAllDataItems(Call call, bool ascending = true)
	{
		var pathIterator = GetPathEnumerable(ascending);
		if (pathIterator == null) return [];

		return pathIterator
			.Select(path => DataRepo.LoadPath<T>(call, path))
			.OfType<DataItem<T>>()
			.Select(dataItem => new DataItem<T>(dataItem.Key, dataItem.Value));
	}
}
