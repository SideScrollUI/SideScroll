using SideScroll.Attributes;
using SideScroll.Utilities;

namespace SideScroll.Serialize.DataRepos;

/// <summary>
/// Holds an in-memory copy of the data repository instance with automatic synchronization
/// </summary>
[Unserialized]
public class DataRepoView<T> : DataRepoInstance<T>
{
	/// <summary>
	/// Gets the in-memory collection of items
	/// </summary>
	public DataItemCollection<T> Items { get; protected set; } = [];

	/// <summary>
	/// Gets all keys in the collection
	/// </summary>
	public IEnumerable<string> Keys => Items.Keys;
	
	/// <summary>
	/// Gets all values in the collection
	/// </summary>
	public IEnumerable<T> Values => Items.Values;

	/// <summary>
	/// Gets whether the items have been loaded from storage
	/// </summary>
	public bool IsLoaded { get; protected set; }

	public DataRepoView(DataRepo dataRepo, string groupId, bool indexed = false, int? maxItems = null)
		: base(dataRepo, groupId, indexed, maxItems)
	{ }

	public DataRepoView(DataRepoInstance<T> dataRepoInstance)
		: base(dataRepoInstance.DataRepo, dataRepoInstance.GroupId)
	{ }

	public override DataItemCollection<T> LoadAll(Call? call = null, bool ascending = true)
	{
		lock (DataRepo)
		{
			Items = base.LoadAll(call, ascending);
			IsLoaded = true;
			return Items;
		}
	}

	/// <summary>
	/// Loads all indexed items into memory
	/// </summary>
	public void LoadAllIndexed(Call call, bool ascending = true, bool force = false)
	{
		lock (DataRepo)
		{
			if (IsLoaded && !force) return;

			if (Index == null)
			{
				LoadAll(call);
				return;
			}

			var dataItems = LoadAllDataItems(call, ascending);
			Items = [.. dataItems];
			IsLoaded = true;
		}
	}

	/// <summary>
	/// Loads all items and orders them by the specified member name
	/// </summary>
	public void LoadAllOrderBy(Call call, string orderByMemberName, bool ascending = true)
	{
		lock (DataRepo)
		{
			DataItemCollection<T> items = base.LoadAll(call);
			var ordered = ascending ? items.OrderBy(orderByMemberName) : items.OrderByDescending(orderByMemberName);
			Items = [.. ordered];
			IsLoaded = true;
		}
	}

	/// <summary>
	/// Sorts the loaded items by the specified member name in ascending order
	/// </summary>
	public void SortBy(string memberName)
	{
		lock (DataRepo)
		{
			var ordered = Items.OrderBy(memberName);
			Items = [.. ordered];
		}
	}

	/// <summary>
	/// Sorts the loaded items by the specified member name in descending order
	/// </summary>
	public void SortByDescending(string memberName)
	{
		lock (DataRepo)
		{
			var ordered = Items.OrderByDescending(memberName);
			Items = [.. ordered];
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
			if (IsLoaded)
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
			if (IsLoaded && item != null)
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

/// <summary>
/// Manages a collection of data repository views organized by group identifier
/// </summary>
public class DataRepoViewCollection<T>(DataRepo dataRepo, string defaultGroupId, string? orderByMemberName = null)
{
	private readonly Dictionary<string, DataRepoView<T>> _dataRepoViews = [];

	/// <summary>
	/// Loads a data repository view for the specified group, using a cached instance if available
	/// </summary>
	public DataRepoView<T> Load(Call call, string? groupId = null)
	{
		groupId ??= defaultGroupId;
		lock (_dataRepoViews)
		{
			if (_dataRepoViews.TryGetValue(groupId, out DataRepoView<T>? existingDataRepo)) return existingDataRepo;

			var dataRepoView = dataRepo.LoadView<T>(call, groupId, orderByMemberName);

			_dataRepoViews.Add(groupId, dataRepoView);
			return dataRepoView;
		}
	}
}
