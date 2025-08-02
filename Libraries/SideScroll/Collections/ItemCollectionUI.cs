using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SideScroll.Collections;

public interface IContext
{
	SynchronizationContext? Context { get; set; }
	void InitializeContext(bool reset = false);
}

// Allows updating UI after initialization
public class ItemCollectionUI<T> : ObservableCollection<T>, IList, IItemCollection, IContext //, IRaiseItemChangedEvents //
{
	public string? Label { get; set; }
	public string? ColumnName { get; set; }
	public string? CustomSettingsPath { get; set; }
	public object? DefaultSelectedItem { get; set; }
	public bool Skippable { get; set; } = true;
	public bool? ShowHeader { get; set; }

	// Enable for thread safety when there's multiple threads acting on this collection
	// true:  Always post new Events to the context
	// false: Events can shortcut and run on the current context, bypassing the event queue, which can be faster, but out of order
	public bool PostOnly { get; set; }

	public SynchronizationContext? Context { get; set; } // TabInstance will initialize this, don't want to initialize this early due to default SynchronizationContext not posting messages in order

	public bool UsePost => Context != null && (PostOnly || Context != SynchronizationContext.Current);

	private readonly object _lock = new();

	public ItemCollectionUI() { }

	// Don't implement List<T>, it isn't sortable
	public ItemCollectionUI(IEnumerable<T> enumerable) :
		base(enumerable)
	{
	}

	public void InitializeContext(bool reset = false)
	{
		if (Context == null || reset)
		{
			Context = SynchronizationContext.Current ?? new();
		}
	}

	public new void Add(T item)
	{
		if (UsePost)
		{
			// Add later so we don't insert at the same index for multiple Adds()
			Context!.Post(AddItemCallback, item);
		}
		else
		{
			AddItemCallback(item);
		}
	}

	// Thread safe callback
	private void AddItemCallback(object? state)
	{
		// Debug.Print("AddItemCallback: Item = " + state.ToString());
		T item = (T)state!;
		lock (_lock)
		{
			base.Add(item);
		}
	}

	public void AddRange(IEnumerable<T> collection)
	{
		if (UsePost)
		{
			Context!.Post(AddRangeCallback, collection);
		}
		else
		{
			AddRangeCallback(collection);
		}
	}

	// Thread safe callback, only works if the context is the same
	// Todo: Improve efficiency, single item Add is slow
	private void AddRangeCallback(object? state)
	{
		if (state is IEnumerable<T> collection)
		{
			lock (_lock)
			{
				foreach (T item in collection)
				{
					base.Add(item);
				}
			}
		}
	}


	public void Replace(IEnumerable<T> collection)
	{
		if (UsePost)
		{
			Context!.Post(ReplaceCallback, collection);
		}
		else
		{
			ReplaceCallback(collection);
		}
	}

	// Thread safe callback, only works if the context is the same
	// Todo: Improve efficiency, single item Add is slow and triggers new item selections
	private void ReplaceCallback(object? state)
	{
		if (state is IEnumerable<T> collection)
		{
			lock (_lock)
			{
				base.Clear();
				foreach (T item in collection)
				{
					base.Add(item);
				}
			}
		}
	}

	public readonly record struct ItemLocation(int Index, T Item);

	// Overriding InsertItem() has out of order issues, so override this instead
	public new void Insert(int index, T item)
	{
		var location = new ItemLocation(index, item);

		if (UsePost)
		{
			// Debug.Print("InsertItem -> Post -> InsertItemCallback: Index = " + index + ", Item = " + item.ToString());
			Context!.Post(InsertItemCallback, location); // default context inserts multiple items in wrong order, AvaloniaUI doesn't
		}
		else
		{
			InsertItemCallback(location);
		}
	}

	// Thread safe callback, only works if the context is the same
	private void InsertItemCallback(object? state)
	{
		ItemLocation itemLocation = (ItemLocation)state!;

		lock (_lock)
		{
			// Debug.Print("InsertItemCallback: Index = " + itemLocation.Index + ", Item = " + itemLocation.Item.ToString());
			base.InsertItem(itemLocation.Index, itemLocation.Item);
		}
	}

	public new void Clear()
	{
		if (UsePost)
		{
			Context!.Post(ClearCallback, null);
		}
		else
		{
			base.Clear();
		}
	}

	// Thread safe callback, only works if the context is the same
	private void ClearCallback(object? state)
	{
		lock (_lock)
		{
			base.Clear();
		}
	}

	protected override void RemoveItem(int index)
	{
		if (UsePost)
		{
			Context!.Post(RemoveItemCallback, index);
		}
		else
		{
			RemoveItemCallback(index);
		}
	}

	// Thread safe callback, only works if the context is the same
	private void RemoveItemCallback(object? state)
	{
		int index = (int)state!;

		lock (_lock)
		{
			base.RemoveItem(index);
		}
	}

	public void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		OnCollectionChanged(e);
	}
}
