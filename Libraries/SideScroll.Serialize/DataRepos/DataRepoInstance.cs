using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;
using SideScroll.Utilities;

namespace SideScroll.Serialize.DataRepos;

public interface IDataRepoInstance
{
	string GroupId { get; }

	Type DataType { get; }

	//object GetObject(string key);
}

[Unserialized]
public class DataRepoInstance<T> : IDataRepoInstance
{
	protected const string DefaultKey = ".Default"; // todo: support multiple directory levels?

	public DataRepo DataRepo { get; init; }

	public string GroupId { get; init; }
	public string GroupPath => DataRepo.GetGroupPath(typeof(T), GroupId);

	public Type DataType => typeof(T);

	public DataRepoIndexInstance<T>? Index { get; set; }

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

	public void AddIndex(int? maxItems = null)
	{
		Index ??= new(this, maxItems);
	}

	public virtual void Save(Call? call, T item)
	{
		Save(call, DefaultKey, item);
	}

	public virtual void Save(Call? call, IEnumerable<T> items)
	{
		foreach (var item in items)
		{
			Save(call, item);
		}
	}

	public virtual void Save(Call? call, string key, T item)
	{
		call ??= new();
		Index?.Add(call, key);
		DataRepo.Save<T>(GroupId, key, item, call);
	}

	public virtual T? Load(Call? call, string? key = null, bool createIfNeeded = false, bool lazy = false)
	{
		return DataRepo.Load<T>(GroupId, key ?? DefaultKey, call, createIfNeeded, lazy);
	}

	public virtual DataPageView<T> LoadPageView(Call? call, bool ascending = true)
	{
		return new DataPageView<T>(this, ascending);
	}

	public virtual DataItemCollection<T> LoadAll(Call? call = null, bool ascending = true)
	{
		call ??= new();
		return new DataItemCollection<T>(LoadAllDataItems(call, ascending));
	}

	public List<Header> LoadHeaders(Call? call = null)
	{
		return DataRepo.LoadHeaders(typeof(T), GroupId, call);
	}

	public virtual void Delete(Call? call, T item)
	{
		string key = ObjectUtils.GetObjectId(item)!;
		Delete(call, key);
	}

	public virtual void Delete(Call? call = null, string? key = null)
	{
		call ??= new();
		key ??= DefaultKey;
		Index?.Remove(call, key);
		DataRepo.Delete<T>(call, GroupId, key);
	}

	public virtual void DeleteAll(Call? call)
	{
		call ??= new();
		Index?.RemoveAll(call);
		DataRepo.DeleteAll<T>(call, GroupId);
	}

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

		if (ascending)
		{
			return enumerable;
		}
		else
		{
			return enumerable.Reverse();
		}
	}

	public IEnumerable<DataItem<T>> LoadAllDataItems(Call call, bool ascending = false)
	{
		var pathIterator = GetPathEnumerable(ascending);
		if (pathIterator == null) return [];

		return pathIterator
			.Select(path => DataRepo.LoadPath<T>(call, path))
			.OfType<DataItem<T>>()
			.Select(dataItem => new DataItem<T>(dataItem.Key, dataItem.Value));
	}
}
