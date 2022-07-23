using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Atlas.Serialize;

public interface IDataView
{
	event EventHandler<EventArgs>? OnDelete;

	void Load(object sender, object obj, params object[] loadParams);
}

// An Item collection that shows a View around every item
public class DataViewCollection<TDataType, TViewType> where TViewType : IDataView, new()
{
	//public event EventHandler<EventArgs> OnDelete; // todo?

	public string? Path;
	public ItemCollectionUI<TViewType> Items { get; set; } = new();

	public DataRepoView<TDataType> DataRepoView;
	public DataRepoView<TDataType>? DataRepoSecondary; // Optional: Saves and Deletes goto a 2nd copy

	public object[] LoadParams;

	private Dictionary<TViewType, IDataItem> _dataItemLookup;
	private Dictionary<IDataItem, TViewType> _valueLookup;

	public DataViewCollection(DataRepoView<TDataType> dataRepoView, params object[] loadParams)
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

		_dataItemLookup = new();
		_valueLookup = new();

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
		else if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			Items.Clear();
		}
	}

	public TViewType Add(IDataItem dataItem)
	{
		var itemView = new TViewType();
		itemView.Load(this, dataItem.Object!, LoadParams);
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
		DataRepoView.Delete(dataItem.Key);
		DataRepoSecondary?.Delete(dataItem.Key);

		if (_valueLookup.TryGetValue(dataItem, out TViewType? existing))
		{
			_valueLookup.Remove(dataItem);
			_dataItemLookup.Remove(existing);
			Items.Remove(existing);
		}
	}

	public void AddDataRepo(DataRepoView<TDataType> dataRepoView)
	{
		DataRepoSecondary = dataRepoView;
	}
}
