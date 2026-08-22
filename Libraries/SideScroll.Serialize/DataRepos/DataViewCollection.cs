using SideScroll.Collections;
using SideScroll.Tasks;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Serialize.DataRepos;

/// <summary>
/// Base interface for data views that can be loaded into a <see cref="DataViewCollection{TDataType,TViewType}"/>.
/// Implement this on new view classes — delete support is managed at the collection level via
/// <see cref="DataViewCollection{TDataType, TViewType}.EnableDeleting"/>.
/// </summary>
public interface IDataViewItem
{
	/// <summary>
	/// Loads the view with the specified object and parameters.
	/// </summary>
	void Load(object sender, object obj, params object?[] loadParams);
}

/// <summary>
/// Legacy interface for data views that handle deletion via a per-item event.
/// Extends <see cref="IDataViewItem"/> with an <see cref="OnDelete"/> event that
/// <see cref="DataViewCollection{TDataType, TViewType}"/> subscribes to automatically.
/// New view classes should implement <see cref="IDataViewItem"/> instead and set
/// <see cref="DataViewCollection{TDataType, TViewType}.EnableDeleting"/> on the collection.
/// </summary>
public interface IDataView : IDataViewItem
{
	/// <summary>
	/// Occurs when the view item's delete action is triggered (legacy per-item delete path).
	/// </summary>
	event EventHandler<EventArgs>? OnDelete;
}

/// <summary>
/// UI collection that wraps data items in view objects for display.
/// Dispose it to stop mirroring the repository view, which outlives it when cached.
/// </summary>
public class DataViewCollection<TDataType, TViewType> : IDisposable where TViewType : IDataViewItem, new()
{
	/// <summary>
	/// Gets the UI collection of view items.
	/// </summary>
	public ItemCollectionUI<TViewType> Items { get; } = [];

	/// <summary>
	/// Gets the primary data repository view.
	/// </summary>
	public DataRepoView<TDataType> DataRepoView { get; }

	/// <summary>
	/// Gets or sets the optional secondary repository for saves and deletes.
	/// </summary>
	public DataRepoView<TDataType>? DataRepoSecondary { get; set; }

	/// <summary>
	/// Gets the parameters used when loading view items.
	/// </summary>
	public object?[] LoadParams { get; }

	/// <summary>
	/// Gets or sets whether a delete button is shown for each row in the data grid.
	/// When <see langword="true"/>, clicking the delete button removes the item from both
	/// the UI collection and the underlying data repository and raises <see cref="OnDelete"/>.
	/// </summary>
	public bool EnableDeleting
	{
		get => Items.EnableDeleting;
		set
		{
			Items.EnableDeleting = value;
			Items.OnDelete = value ? DeleteByViewObject : null;
		}
	}

	/// <summary>
	/// Gets or sets the confirmation prompt to show before deleting a row,
	/// or <see langword="null"/> to delete without confirming.
	/// </summary>
	public IFlyoutConfig? DeleteConfirmation
	{
		get => Items.DeleteConfirmation;
		set => Items.DeleteConfirmation = value;
	}

	/// <summary>
	/// Raised on the collection once for each item removed from it, whether the delete started here
	/// or in the repository. An item this collection isn't displaying doesn't raise it.
	/// </summary>
	public event EventHandler<EventArgs>? OnDelete;

	private Dictionary<TViewType, IDataItem> _dataItemLookup;
	private Dictionary<IDataItem, TViewType> _valueLookup;

	/// <summary>Returns the backing <see cref="DataRepoView"/>'s string representation.</summary>
	public override string ToString() => DataRepoView.ToString();

	/// <summary>Initializes a new <see cref="DataViewCollection{TDataType,TViewType}"/> backed by <paramref name="dataRepoView"/>, using <paramref name="loadParams"/> when constructing each view item, and performs an initial load.</summary>
	public DataViewCollection(DataRepoView<TDataType> dataRepoView, params object?[] loadParams)
	{
		DataRepoView = dataRepoView;
		LoadParams = loadParams;

		DataRepoView.Items.CollectionChanged += Items_CollectionChanged;

		Reload();
	}

	/// <summary>
	/// Stops mirroring <see cref="DataRepoView"/>. A repository view is often cached and outlives
	/// the collections built from it, which retains every undisposed one.
	/// </summary>
	public void Dispose()
	{
		DataRepoView.Items.CollectionChanged -= Items_CollectionChanged;
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Reloads all items from the data repository.
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
	/// Adds a new view item for the specified data item.
	/// When the view item implements the legacy <see cref="IDataView"/> interface, its
	/// <see cref="IDataView.OnDelete"/> event is subscribed to for backwards compatibility.
	/// </summary>
	public TViewType Add(IDataItem dataItem)
	{
		var itemView = new TViewType();
		itemView.Load(this, dataItem.Object, LoadParams);

		// Legacy: if the view item raises OnDelete itself, wire it to Remove
		if (itemView is IDataView legacyView)
		{
			legacyView.OnDelete += (_, _) => Remove(dataItem);
		}

		Items.Add(itemView);
		_dataItemLookup.Add(itemView, dataItem);
		_valueLookup.Add(dataItem, itemView);

		return itemView;
	}

	/// <summary>
	/// Inserts a new view item at the specified index.
	/// When the view item implements the legacy <see cref="IDataView"/> interface, its
	/// <see cref="IDataView.OnDelete"/> event is subscribed to for backwards compatibility.
	/// </summary>
	public TViewType Insert(int index, IDataItem dataItem)
	{
		var itemView = new TViewType();
		itemView.Load(this, dataItem.Object, LoadParams);

		// Legacy: if the view item raises OnDelete itself, wire it to Remove
		if (itemView is IDataView legacyView)
		{
			legacyView.OnDelete += (_, _) => Remove(dataItem);
		}

		Items.Insert(index, itemView);
		_dataItemLookup.Add(itemView, dataItem);
		_valueLookup.Add(dataItem, itemView);

		return itemView;
	}

	/// <summary>
	/// Delete callback wired to <see cref="Items"/>.<see cref="IDeletableList.OnDelete"/>
	/// when <see cref="EnableDeleting"/> is <see langword="true"/>.
	/// </summary>
	private void DeleteByViewObject(object viewObj)
	{
		if (viewObj is TViewType viewItem && _dataItemLookup.TryGetValue(viewItem, out IDataItem? dataItem))
		{
			Remove(dataItem);
		}
	}

	/// <summary>
	/// Removes the view item associated with the specified data item.
	/// </summary>
	public void Remove(IDataItem dataItem)
	{
		Call call = new();

		// Deleting from a loaded repository removes the item from its collection, and the change
		// notification removes the view item. Calling RemoveView() here as well would delete it twice
		DataRepoView.Delete(call, dataItem.Key);
		DataRepoSecondary?.Delete(call, dataItem.Key);

		// Only removes anything when the repository didn't notify, i.e. it isn't loaded or was already missing the key
		RemoveView(dataItem);
	}

	// Removes the view item and its lookup entries, raising OnDelete only when one was removed,
	// so a delete reports once whether the repository notification or the caller reached it first
	private bool RemoveView(IDataItem dataItem)
	{
		if (!_valueLookup.Remove(dataItem, out TViewType? existing)) return false;

		_dataItemLookup.Remove(existing);
		Items.Remove(existing);

		OnDelete?.Invoke(this, EventArgs.Empty);
		return true;
	}

	/// <summary>
	/// Replaces an old data item with a new one.
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
	/// Adds a secondary data repository for saves and deletes.
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
					// The repository already removed the item, only the view is left to update
					RemoveView(item);
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
			// Rebuild from the repository's current contents, clearing Items alone would leave the
			// lookups holding view items that are no longer displayed
			Reload();
		}
	}
}

/// <summary>
/// Provides a UI thread-safe collection wrapper around a data repository view.
/// Dispose it to stop mirroring the repository view, which outlives it when cached.
/// </summary>
public class DataViewCollection<T> : IDisposable
{
	/// <summary>
	/// Gets the UI collection of items.
	/// </summary>
	public ItemCollectionUI<T> Items { get; } = [];

	/// <summary>
	/// Gets the underlying data repository view.
	/// </summary>
	public DataRepoView<T> DataRepoView { get; }

	/// <summary>
	/// Gets or sets whether a delete button is shown for each row in the data grid.
	/// When <see langword="true"/>, clicking the delete button removes the item from both
	/// the UI collection and the underlying data repository and raises <see cref="OnDelete"/>.
	/// </summary>
	public bool EnableDeleting
	{
		get => Items.EnableDeleting;
		set
		{
			Items.EnableDeleting = value;
			Items.OnDelete = value ? DeleteByObject : null;
		}
	}

	/// <summary>
	/// Gets or sets the confirmation prompt to show before deleting a row,
	/// or <see langword="null"/> to delete without confirming.
	/// </summary>
	public IFlyoutConfig? DeleteConfirmation
	{
		get => Items.DeleteConfirmation;
		set => Items.DeleteConfirmation = value;
	}

	/// <summary>
	/// Raised on the collection once for each item removed from it, whether the delete started here
	/// or in the repository. An item this collection isn't displaying doesn't raise it.
	/// </summary>
	public event EventHandler<EventArgs>? OnDelete;

	private Dictionary<IDataItem, T> _valueLookup;

	/// <summary>Returns the backing <see cref="DataRepoView"/>'s string representation.</summary>
	public override string ToString() => DataRepoView.ToString();

	/// <summary>Initializes a new collection backed by <paramref name="dataRepoView"/> and performs an initial load.</summary>
	/// <param name="dataRepoView">The data repository view whose items this collection mirrors.</param>
	public DataViewCollection(DataRepoView<T> dataRepoView)
	{
		DataRepoView = dataRepoView;

		DataRepoView.Items.CollectionChanged += Items_CollectionChanged;

		Reload();
	}

	/// <summary>
	/// Stops mirroring <see cref="DataRepoView"/>. A repository view is often cached and outlives
	/// the collections built from it, which retains every undisposed one.
	/// </summary>
	public void Dispose()
	{
		DataRepoView.Items.CollectionChanged -= Items_CollectionChanged;
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Reloads all items from the data repository.
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
	/// Adds a new item to the collection.
	/// </summary>
	public T Add(IDataItem dataItem)
	{
		var item = (T)dataItem.Object;
		Items.Add(item);
		_valueLookup.Add(dataItem, item);

		return item;
	}

	/// <summary>
	/// Delete callback wired to <see cref="Items"/>.<see cref="IDeletableList.OnDelete"/>
	/// when <see cref="EnableDeleting"/> is <see langword="true"/>.
	/// </summary>
	private void DeleteByObject(object obj)
	{
		if (obj is not T item)
			return;

		// Reverse-lookup: find the IDataItem whose stored value is this object instance.
		IDataItem? dataItem = _valueLookup
			.FirstOrDefault(kvp => EqualityComparer<T>.Default.Equals(kvp.Value, item))
			.Key;

		if (dataItem != null)
		{
			Remove(dataItem);
		}
	}

	/// <summary>
	/// Removes the specified data item from the collection.
	/// </summary>
	public void Remove(IDataItem dataItem)
	{
		Call call = new();

		// Deleting from a loaded repository removes the item from its collection, and the change
		// notification removes the value. Calling RemoveValue() here as well would delete it twice
		DataRepoView.Delete(call, dataItem.Key);

		// Only removes anything when the repository didn't notify, i.e. it isn't loaded or was already missing the key
		RemoveValue(dataItem);
	}

	// Removes the value, raising OnDelete only when one was removed, so a delete reports once
	// whether the repository notification or the caller reached it first
	private bool RemoveValue(IDataItem dataItem)
	{
		bool removed;
		if (_valueLookup.Remove(dataItem, out T? existing))
		{
			removed = Items.Remove(existing);

			if (dataItem.Object is T obj && !Equals(existing, obj))
			{
				removed |= Items.Remove(obj);
			}
		}
		else
		{
			removed = dataItem.Object is T obj && Items.Remove(obj);
		}

		if (removed)
		{
			OnDelete?.Invoke(this, EventArgs.Empty);
		}
		return removed;
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
					// The repository already removed the item, only the view is left to update
					RemoveValue(item);
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
			// Rebuild from the repository's current contents, clearing Items alone would leave the
			// lookup holding values that are no longer displayed
			Reload();
		}
	}
}
