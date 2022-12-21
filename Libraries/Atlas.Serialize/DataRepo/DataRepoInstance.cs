using Atlas.Core;

namespace Atlas.Serialize;

public interface IDataRepoInstance
{
	string GroupId { get; }

	//object GetObject(string key);
}

[Unserialized]
public class DataRepoInstance<T> : IDataRepoInstance
{
	protected const string DefaultKey = ".Default"; // todo: support multiple directory levels?

	public readonly DataRepo DataRepo;
	public string GroupId { get; set; }

	public override string ToString() => GroupId;

	public DataRepoInstance(DataRepo dataRepo, string groupId)
	{
		DataRepo = dataRepo;
		GroupId = groupId;
	}

	public virtual void Save(Call? call, T item)
	{
		DataRepo.Save<T>(GroupId, DefaultKey, item, call);
	}

	public virtual void Save(Call? call, string key, T item)
	{
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

	public virtual void Delete(Call? call, T item)
	{
		Delete(call, item!.ToString());
	}

	public virtual void Delete(Call? call = null, string? key = null)
	{
		DataRepo.Delete<T>(call, GroupId, key ?? DefaultKey);
	}

	public virtual void DeleteAll(Call? call)
	{
		DataRepo.DeleteAll<T>(call, GroupId);
	}
}
