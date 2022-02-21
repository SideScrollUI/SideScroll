using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace Atlas.Core;

public interface IContext
{
	SynchronizationContext Context { get; set; }
	void InitializeContext(bool reset = false);
}

// Allows updating UI after initialization
public class ItemCollectionUI<T> : ObservableCollection<T>, IList, IItemCollection, IContext //, IRaiseItemChangedEvents //
{
	public string ColumnName { get; set; }
	public string Label { get; set; }
	public string CustomSettingsPath { get; set; }
	public bool Skippable { get; set; } = true;

	// Enable for thread safety when there's multiple threads acting on this collection
	// true:  Always post new Events to the context
	// false: Events can shortcut and run on the current context, bypassing the event queue, which can be faster, but out of order
	public bool PostOnly { get; set; } = false;

	public SynchronizationContext Context { get; set; } // TabInstance will initialize this, don't want to initialize this early due to default SynchronizationContext not posting messages in order

	private readonly object _lock = new();

	public ItemCollectionUI()
	{
	}

	// Don't implement List<T>, it isn't sortable
	public ItemCollectionUI(IEnumerable<T> iEnumerable) :
		base(iEnumerable)
	{
	}

	public void InitializeContext(bool reset = false)
	{
		if (Context == null || reset)
		{
			Context = SynchronizationContext.Current ?? new SynchronizationContext();
		}
	}

	public new void Add(T item)
	{
		if (Context == null)
		{
			AddItemCallback(item);
		}
		else if (PostOnly || Context != SynchronizationContext.Current)
		{
			// Add later so we don't insert at the same index for multiple Adds()
			Context.Post(new SendOrPostCallback(AddItemCallback), item);
		}
		else
		{
			AddItemCallback(item);
		}
	}

	// Thread safe callback
	private void AddItemCallback(object state)
	{
		// Debug.Print("AddItemCallback: Item = " + state.ToString());
		T item = (T)state;
		lock (_lock)
		{
			base.Add(item);
		}
	}

	public void AddRange(IEnumerable<T> collection)
	{
		int index = Items.Count;
		foreach (T item in collection)
		{
			InsertItem(index++, item); // item gets added in the background with Add() and doesn't increment index
		}

		//foreach (T item in collection)
		//	Items.Add(item);
		//OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)); // need ui thread
	}

	record struct ItemLocation(int Index, T Item)
	{
		public readonly int Index = Index;
		public readonly T Item = Item;
	}

	// Overriding InsertItem() has out of order issues, so override this instead
	public new void Insert(int index, T item)
	{
		var location = new ItemLocation(index, item);

		if (Context == null)
		{
			InsertItemCallback(location);
		}
		else if (PostOnly || Context != SynchronizationContext.Current)
		{
			// Debug.Print("InsertItem -> Post -> InsertItemCallback: Index = " + index + ", Item = " + item.ToString());
			Context.Post(new SendOrPostCallback(InsertItemCallback), location); // default context inserts multiple items in wrong order, AvaloniaUI doesn't
		}
		else
		{
			InsertItemCallback(location);
		}
	}

	// Thread safe callback, only works if the context is the same
	private void InsertItemCallback(object state)
	{
		ItemLocation itemLocation = (ItemLocation)state;

		lock (_lock)
		{
			// Debug.Print("InsertItemCallback: Index = " + itemLocation.Index + ", Item = " + itemLocation.Item.ToString());
			base.InsertItem(itemLocation.Index, itemLocation.Item);
		}
	}

	public new void Clear()
	{
		if (Context == null || (PostOnly && Context == SynchronizationContext.Current))
		{
			base.Clear();
		}
		else
		{
			Context.Post(new SendOrPostCallback(ClearCallback), null);
		}
	}

	// Thread safe callback, only works if the context is the same
	private void ClearCallback(object state)
	{
		lock (_lock)
		{
			base.Clear();
		}
	}

	protected override void RemoveItem(int index)
	{
		if (Context == null)
		{
			RemoveItemCallback(index);
		}
		else if (PostOnly || Context != SynchronizationContext.Current)
		{
			Context.Post(new SendOrPostCallback(RemoveItemCallback), index);
		}
		else
		{
			RemoveItemCallback(index);
		}
	}

	// Thread safe callback, only works if the context is the same
	private void RemoveItemCallback(object state)
	{
		int index = (int)state;

		lock (_lock)
		{
			base.RemoveItem(index);
		}
	}
}
