using SideScroll.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Serialize.DataRepos;

/// <summary>
/// Interface for data views that can be loaded and deleted
/// </summary>
public interface IDataView
{
	/// <summary>
	/// Occurs when the view is deleted
	/// </summary>
	event EventHandler<EventArgs>? OnDelete;

	/// <summary>
	/// Loads the view with the specified object and parameters
	/// </summary>
	void Load(object sender, object obj, params object?[] loadParams);
}

/// <summary>
/// UI collection that wraps data items in view objects for display
/// </summary>
public class DataViewCollection<TDataType, TViewType> where TViewType : IDataView, new()
{
	//public event EventHandler<EventArgs> OnDelete; // todo?

	/// <summary>
	/// Gets the UI collection of view items
	/// </summary>
	public ItemCollectionUI<TViewType> Items { get; } = [];

	/// <summary>
	/// Gets the primary data repository view
	/// </summary>
	public DataRepoView<TDataType> DataRepoView { get; }
	
	/// <summary>
	/// Gets or sets the optional secondary repository for saves and deletes
	/// </summary>
	public DataRepoView<TDataType>? DataRepoSecondary { get; set; }

	/// <summary>
	/// Gets the parameters used when loading view items
	/// </summary>
	public object?[] LoadParams { get; }

	private Dictionary<TViewType, IDataItem> _dataItemLookup;
	private Dictionary<IDataItem, TViewType> _valueLookup;

	public override string ToString() => DataRepoView.ToString();

	public DataViewCollection(DataRepoView<TDataType> dataRepoView, params object?[] loadParams)
	{
		DataRepoView = dataRepoView;
		LoadParams = loadParams;

		DataRepoView.Items.CollectionChanged += Items_CollectionChanged;

		Reload();
	}

	/// <summary>
	/// Reloads all items from the data repository
	/// </summary>
	[MemberNotNull(nameof(_dataItemLookup), nameof(_valueLookup))]
	public void Reload()
	{
		Items.Clear();

		_dataItemLookup = [];
		_valueLookup = [];

		foreach (DataItem<TDataType> dataItem in DataRepoView.Items)
		{
			Add(dataItem);
		}
	}

	/// <summary>
	/// Adds a new view item for the specified data item
	/// </summary>
	public TViewType Add(IDataItem dataItem)
	{
		var itemView = new TViewType();
		itemView.Load(this, dataItem.Object, LoadParams);
		itemView.OnDelete += Item_OnDelete;

		Items.Add(itemView);
		_dataItemLookup.Add(itemView, dataItem);
		_valueLookup.Add(dataItem, itemView);

		return itemView;
	}

	/// <summary>
	/// Inserts a new view item at the specified index
	/// </summary>
	public TViewType Insert(int index, IDataItem dataItem)
	{
		var itemView = new TViewType();
		itemView.Load(this, dataItem.Object, LoadParams);
		itemView.OnDelete += Item_OnDelete;

		Items.Insert(index, itemView);
		_dataItemLookup.Add(itemView, dataItem);
		_valueLookup.Add(dataItem, itemView);

		return itemView;
	}

	private void Item_OnDelete(object? sender, EventArgs e)
	{
		TViewType tab = (TViewType)sender!;
		if (!_dataItemLookup.TryGetValue(tab, out IDataItem? dataItem))
			return;

		Remove(dataItem);
	}

	/// <summary>
	/// Removes the view item associated with the specified data item
	/// </summary>
	public void Remove(IDataItem dataItem)
	{
		Call call = new();
		DataRepoView.Delete(call, dataItem.Key);
		DataRepoSecondary?.Delete(call, dataItem.Key);

		if (_valueLookup.Remove(dataItem, out TViewType? existing))
		{
			_dataItemLookup.Remove(existing);
			Items.Remove(existing);
		}
	}

	/// <summary>
	/// Replaces an old data item with a new one
	/// </summary>
	public void Replace(IDataItem oldDataItem, IDataItem newDataItem)
	{
		if (_valueLookup.Remove(oldDataItem, out TViewType? existing))
		{
			int index = Items.IndexOf(existing);

			_dataItemLookup.Remove(existing);
			Items.Remove(existing);

			Insert(index, newDataItem);
		}
		else
		{
			Add(newDataItem);
		}
	}

	/// <summary>
	/// Adds a secondary data repository for saves and deletes
	/// </summary>
	public void AddDataRepo(DataRepoView<TDataType> dataRepoView)
	{
		DataRepoSecondary = dataRepoView;
	}

	private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Add)
		{
			if (e.NewItems != null)
			{
				foreach (IDataItem item in e.NewItems)
				{
					Add(item);
				}
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Remove)
		{
			if (e.OldItems != null)
			{
				foreach (IDataItem item in e.OldItems)
				{
					Remove(item);
				}
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Replace)
		{
			if (e.OldItems == null || e.NewItems?.Count != e.OldItems.Count) return;

			int index = 0;
			foreach (IDataItem oldItem in e.OldItems)
			{
				var newItem = (IDataItem)e.NewItems![index]!;
				if (_valueLookup.TryGetValue(oldItem, out TViewType? itemView))
				{
					Replace(oldItem, newItem);
				}
				else
				{
					Add(newItem);
				}
				index++;
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			Items.Clear();
		}
	}
}

/// <summary>
/// Provides a UI thread-safe collection wrapper around a data repository view
/// </summary>
public class DataViewCollection<T>
{
	//public event EventHandler<EventArgs> OnDelete; // todo?

	/// <summary>
	/// Gets the UI collection of items
	/// </summary>
	public ItemCollectionUI<T> Items { get; } = [];

	/// <summary>
	/// Gets the underlying data repository view
	/// </summary>
	public DataRepoView<T> DataRepoView { get; }

	private Dictionary<IDataItem, T> _valueLookup;

	public override string ToString() => DataRepoView.ToString();

	public DataViewCollection(DataRepoView<T> dataRepoView)
	{
		DataRepoView = dataRepoView;

		DataRepoView.Items.CollectionChanged += Items_CollectionChanged;

		Reload();
	}

	/// <summary>
	/// Reloads all items from the data repository
	/// </summary>
	[MemberNotNull(nameof(_valueLookup))]
	public void Reload()
	{
		Items.Clear();

		_valueLookup = [];

		foreach (DataItem<T> dataItem in DataRepoView.Items)
		{
			Add(dataItem);
		}
	}

	/// <summary>
	/// Adds a new item to the collection
	/// </summary>
	public T Add(IDataItem dataItem)
	{
		var item = (T)dataItem.Object;
		Items.Add(item);
		_valueLookup.Add(dataItem, item);

		return item;
	}

	/// <summary>
	/// Removes the specified data item from the collection
	/// </summary>
	public void Remove(IDataItem dataItem)
	{
		Call call = new();
		DataRepoView.Delete(call, dataItem.Key);

		if (_valueLookup.Remove(dataItem, out T? existing))
		{
			Items.Remove(existing);

			if (dataItem.Object is T obj && !Equals(existing, obj))
			{
				Items.Remove(obj);
			}
		}
		else
		{
			Items.Remove((T)dataItem.Object);
		}
	}

	private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Add)
		{
			if (e.NewItems != null)
			{
				foreach (IDataItem item in e.NewItems)
				{
					Add(item);
				}
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Remove)
		{
			if (e.OldItems != null)
			{
				foreach (IDataItem item in e.OldItems)
				{
					Remove(item);
				}
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Replace)
		{
			if (e.OldItems == null || e.NewItems?.Count != e.OldItems.Count) return;

			int index = 0;
			foreach (IDataItem oldItem in e.OldItems)
			{
				var newItem = (IDataItem)e.NewItems![index]!;
				var oldObject = (T)oldItem.Object;
				var newObject = (T)newItem.Object;

				// OldItem can be mapped to newItem so grab the real old value
				if (_valueLookup.TryGetValue(oldItem, out T? itemView))
				{
					Items.Replace(itemView, newObject);
				}
				else if (Items.Contains(oldObject))
				{
					Items.Replace(oldObject, newObject);
				}

				if (newItem != oldItem)
				{
					_valueLookup.Remove(oldItem);
				}
				_valueLookup[newItem] = newObject;

				index++;
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			Items.Clear();
		}
	}
}
