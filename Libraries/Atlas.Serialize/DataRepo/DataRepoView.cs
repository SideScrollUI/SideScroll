using Atlas.Core;

namespace Atlas.Serialize;

// Holds an in memory copy of the DataRepo
[Unserialized]
public class DataRepoView<T> : DataRepoInstance<T>
{
	//public DataRepo<T> DataRepo; // Add template version?

	public DataItemCollection<T> Items { get; set; } = new();

	public DataRepoView(DataRepo dataRepo, string groupId) : base(dataRepo, groupId)
	{
	}

	public DataRepoView(DataRepoInstance<T> dataRepoInstance) : base(dataRepoInstance.DataRepo, dataRepoInstance.GroupId)
	{
	}

	public void LoadAll(Call call)
	{
		lock (DataRepo)
		{
			Items = base.LoadAll(call);
		}
	}

	public void LoadAllOrderBy(Call call, string orderByMemberName, bool ascending = true)
	{
		lock (DataRepo)
		{
			DataItemCollection<T> items = base.LoadAll(call);
			var ordered = ascending ? items.OrderBy(orderByMemberName) : items.OrderByDescending(orderByMemberName);
			Items = new DataItemCollection<T>(ordered);
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
		Save(call, item!.ToString()!, item);
	}

	public override void Save(Call? call, string key, T item)
	{
		lock (DataRepo)
		{
			// Delete(call, key); // Don't trigger delete notifications
			base.Save(call, key, item);
			Items.Update(key, item);
		}
	}

	public override void Delete(Call? call, T item)
	{
		Delete(call, item!.ToString());
	}

	public override void Delete(Call? call, string? key = null)
	{
		key ??= DefaultKey;
		lock (DataRepo)
		{
			base.Delete(call, key);

			var item = Items.FirstOrDefault(d => d.Key == key);
			if (item != null)
				Items.Remove(item);
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
