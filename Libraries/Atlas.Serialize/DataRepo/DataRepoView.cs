using Atlas.Core;
using Atlas.Core.Utilities;

namespace Atlas.Serialize;

// Holds an in memory copy of the DataRepoInstance
[Unserialized]
public class DataRepoView<T> : DataRepoInstance<T>
{
	//public DataRepo<T> DataRepo; // Add template version?

	public DataItemCollection<T> Items { get; set; } = [];
	public bool Loaded { get; set; }

	public DataRepoView(DataRepo dataRepo, string groupId, bool indexed = false, int? maxItems = null)
		: base(dataRepo, groupId, indexed, maxItems)
	{ }

	public DataRepoView(DataRepoInstance<T> dataRepoInstance)
		: base(dataRepoInstance.DataRepo, dataRepoInstance.GroupId)
	{ }

	public void LoadAll(Call call)
	{
		lock (DataRepo)
		{
			Items = base.LoadAll(call);
			Loaded = true;
		}
	}

	public void LoadAllIndexed(Call call, bool ascending = true, bool force = false)
	{
		lock (DataRepo)
		{
			if (Loaded && !force) return;

			if (Index == null)
			{
				LoadAll(call);
				return;
			}

			var dataItems = LoadAllDataItems(call, ascending);
			Items = new DataItemCollection<T>(dataItems);
			Loaded = true;
		}
	}

	public void LoadAllOrderBy(Call call, string orderByMemberName, bool ascending = true)
	{
		lock (DataRepo)
		{
			DataItemCollection<T> items = base.LoadAll(call);
			var ordered = ascending ? items.OrderBy(orderByMemberName) : items.OrderByDescending(orderByMemberName);
			Items = new DataItemCollection<T>(ordered);
			Loaded = true;
		}
	}

	public void SortBy(string memberName)
	{
		lock (DataRepo)
		{
			var ordered = Items.OrderBy(memberName);
			Items = new DataItemCollection<T>(ordered);
		}
	}

	public void SortByDescending(string memberName)
	{
		lock (DataRepo)
		{
			var ordered = Items.OrderByDescending(memberName);
			Items = new DataItemCollection<T>(ordered);
		}
	}

	public override void Save(Call? call, T item)
	{
		string key = ObjectUtils.GetObjectId(item)!;
		Save(call, key, item);
	}

	public override void Save(Call? call, string key, T item)
	{
		lock (DataRepo)
		{
			// Delete(call, key); // Don't trigger delete notifications
			base.Save(call, key, item);
			if (Loaded)
			{
				Items.Update(key, item);
			}
		}
	}

	public override void Delete(Call? call, T item)
	{
		string key = ObjectUtils.GetObjectId(item)!;
		Delete(call, key);
	}

	public override void Delete(Call? call = null, string? key = null)
	{
		key ??= DefaultKey;
		lock (DataRepo)
		{
			base.Delete(call, key);

			var item = Items.FirstOrDefault(d => d.Key == key);
			if (Loaded && item != null)
			{
				Items.Remove(item);
			}
		}
	}

	public override void DeleteAll(Call? call)
	{
		lock (DataRepo)
		{
			base.DeleteAll(call);
			Items.Clear();
		}
	}
}
