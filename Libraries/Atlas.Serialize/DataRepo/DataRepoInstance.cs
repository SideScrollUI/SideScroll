using Atlas.Core;

namespace Atlas.Serialize;

public interface IDataRepoInstance
{
	string GroupId { get; }

	Type DataType { get; }

	//object GetObject(string key);
}

[Unserialized]
public class DataRepoInstance<T>(DataRepo dataRepo, string groupId) : IDataRepoInstance
{
	protected const string DefaultKey = ".Default"; // todo: support multiple directory levels?

	public readonly DataRepo DataRepo = dataRepo;
	public string GroupId { get; set; } = groupId;
	public string GroupPath => DataRepo.GetGroupPath(typeof(T), GroupId);
	public Type DataType => typeof(T);

	public override string ToString() => GroupId;

	public virtual void Save(Call? call, T item)
	{
		Save(call, DefaultKey, item);
	}

	public virtual void Save(Call? call, string key, T item)
	{
		call ??= new();
		DataRepo.Save<T>(GroupId, key, item, call);
	}

	public virtual T? Load(Call? call, string? key = null, bool createIfNeeded = false, bool lazy = false)
	{
		return DataRepo.Load<T>(GroupId, key ?? DefaultKey, call, createIfNeeded, lazy);
	}

	public DataItemCollection<T> LoadAll(Call? call = null, bool lazy = false)
	{
		return DataRepo.LoadAll<T>(call, GroupId, lazy);
	}

	public ItemCollection<Header> LoadHeaders(Call? call = null)
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
		DataRepo.Delete<T>(call, GroupId, key);
	}

	public virtual void DeleteAll(Call? call)
	{
		call ??= new();
		DataRepo.DeleteAll<T>(call, GroupId);
	}
}
