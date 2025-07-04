using SideScroll.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Serialize.DataRepos;

public interface IDataView
{
	event EventHandler<EventArgs>? OnDelete;

	void Load(object sender, object obj, params object?[] loadParams);
}

// A UI Item Collection that shows a View around every item
public class DataViewCollection<TDataType, TViewType> where TViewType : IDataView, new()
{
	//public event EventHandler<EventArgs> OnDelete; // todo?

	public ItemCollectionUI<TViewType> Items { get; set; } = [];

	public DataRepoView<TDataType> DataRepoView { get; }
	public DataRepoView<TDataType>? DataRepoSecondary { get; set; } // Optional: Saves and Deletes goto a 2nd copy

	public object?[] LoadParams { get; set; }

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

			List<TViewType> viewItems = [];
			int index = 0;
			foreach (IDataItem oldItem in e.OldItems)
			{
				if (_valueLookup.TryGetValue(oldItem, out TViewType? itemView))
				{
					var newItem = (IDataItem)e.NewItems![index]!;
					itemView.Load(this, newItem.Object, LoadParams);
					viewItems.Add(itemView);
				}
				index++;
			}

			if (viewItems.Count > 0)
			{
				int indexOfItem = Items.IndexOf(viewItems.First());
				Items.NotifyCollectionChanged(
					new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Replace,
						viewItems,
						viewItems,
						indexOfItem));
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			Items.Clear();
		}
	}

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

	private void Item_OnDelete(object? sender, EventArgs e)
	{
		TViewType tab = (TViewType)sender!;
		if (!_dataItemLookup.TryGetValue(tab, out IDataItem? dataItem))
			return;

		Remove(dataItem);
	}

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

	public void AddDataRepo(DataRepoView<TDataType> dataRepoView)
	{
		DataRepoSecondary = dataRepoView;
	}
}

// Provides a UI thread safe ItemCollectionUI around a DataRepoView
public class DataViewCollection<T>
{
	//public event EventHandler<EventArgs> OnDelete; // todo?

	public ItemCollectionUI<T> Items { get; set; } = [];

	public DataRepoView<T> DataRepoView { get; }

	public override string ToString() => DataRepoView.ToString();

	public DataViewCollection(DataRepoView<T> dataRepoView)
	{
		DataRepoView = dataRepoView;

		DataRepoView.Items.CollectionChanged += Items_CollectionChanged;

		Reload();
	}

	public void Reload()
	{
		Items.Clear();

		foreach (DataItem<T> dataItem in DataRepoView.Items)
		{
			Add(dataItem);
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

			List<T> newItems = [];
			int index = 0;
			foreach (IDataItem oldItem in e.OldItems)
			{
				if (Items.Contains((T)oldItem.Object))
				{
					var newItem = (IDataItem)e.NewItems![index]!;
					newItems.Add((T)newItem.Object);
				}
				index++;
			}

			if (newItems.Count > 0)
			{
				int indexOfItem = Items.IndexOf(newItems.First());
				Items.NotifyCollectionChanged(
					new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Replace,
						newItems,
						newItems,
						indexOfItem));
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			Items.Clear();
		}
	}

	public T Add(IDataItem dataItem)
	{
		var item = (T)dataItem.Object;
		Items.Add(item);

		return item;
	}

	public void Remove(IDataItem dataItem)
	{
		Call call = new();
		DataRepoView.Delete(call, dataItem.Key);

		Items.Remove((T)dataItem.Object);
	}
}
