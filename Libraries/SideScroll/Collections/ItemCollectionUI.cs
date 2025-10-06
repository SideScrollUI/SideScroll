using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SideScroll.Collections;

/// <summary>
/// Interface for objects that manage a SynchronizationContext for thread-safe UI operations
/// </summary>
public interface IContext
{
	/// <summary>
	/// The SynchronizationContext used for marshalling operations to the UI thread
	/// </summary>
	SynchronizationContext? Context { get; set; }
	
	/// <summary>
	/// Initializes the SynchronizationContext for this object
	/// </summary>
	void InitializeContext(bool reset = false);
}

/// <summary>
/// A thread-safe observable collection for UI updates. Allows safely adding/removing items from background threads by marshalling operations to the UI thread via SynchronizationContext.
/// Use this instead of ItemCollection when updating the collection from multiple threads or background tasks.
/// </summary>
public class ItemCollectionUI<T> : ObservableCollection<T>, IList, IItemCollection, IContext //, IRaiseItemChangedEvents //
{
	/// <summary>
	/// The label to display for this collection
	/// </summary>
	public string? Label { get; set; }
	
	/// <summary>
	/// The name to display for the first column in the DataGrid
	/// </summary>
	public string? ColumnName { get; set; }
	
	/// <summary>
	/// Custom path for saving/loading tab view settings
	/// </summary>
	public string? CustomSettingsPath { get; set; }
	
	/// <summary>
	/// The default item to select when the collection is displayed
	/// </summary>
	public object? DefaultSelectedItem { get; set; }
	
	/// <summary>
	/// Whether single-item collections should be automatically skipped/expanded in the UI (default: true)
	/// </summary>
	public bool Skippable { get; set; } = true;
	
	/// <summary>
	/// Override default header visibility - when set, always shows/hides the header regardless of other settings
	/// </summary>
	public bool? ShowHeader { get; set; }

	/// <summary>
	/// Controls thread safety behavior:
	/// - true: Always posts events to the UI context (safer, maintains order)
	/// - false: Events can run on current context if already on UI thread (faster, but may be out of order)
	/// </summary>
	public bool PostOnly { get; set; }

	/// <summary>
	/// The SynchronizationContext used for marshalling operations to the UI thread. Initialized by TabInstance.
	/// </summary>
	public SynchronizationContext? Context { get; set; }

	/// <summary>
	/// Gets whether operations should be posted to the UI context based on PostOnly flag and current thread context
	/// </summary>
	public bool UsePost => Context != null && (PostOnly || Context != SynchronizationContext.Current);

	private readonly object _lock = new();

	public ItemCollectionUI() { }

	/// <summary>
	/// Initializes a new ItemCollectionUI with items from an enumerable collection
	/// </summary>
	public ItemCollectionUI(IEnumerable<T> enumerable) :
		base(enumerable)
	{
	}

	/// <summary>
	/// Initializes or resets the SynchronizationContext to the current thread's context
	/// </summary>
	public void InitializeContext(bool reset = false)
	{
		if (Context == null || reset)
		{
			Context = SynchronizationContext.Current ?? new();
		}
	}

	/// <summary>
	/// Adds an item to the collection. Thread-safe - marshals to UI thread if called from background thread.
	/// </summary>
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

	/// <summary>
	/// Adds multiple items to the collection. Thread-safe - marshals to UI thread if called from background thread.
	/// </summary>
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

	/// <summary>
	/// Replaces all items in the collection with the specified items. Thread-safe - marshals to UI thread if called from background thread.
	/// </summary>
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

	/// <summary>
	/// Record struct holding an item and its insertion index for thread-safe insertion operations
	/// </summary>
	public readonly record struct ItemLocation(int Index, T Item);

	/// <summary>
	/// Inserts an item at the specified index. Thread-safe - marshals to UI thread if called from background thread.
	/// </summary>
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

	/// <summary>
	/// Clears all items from the collection. Thread-safe - marshals to UI thread if called from background thread.
	/// </summary>
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

	/// <summary>
	/// Removes the item at the specified index. Thread-safe - marshals to UI thread if called from background thread.
	/// </summary>
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

	/// <summary>
	/// Raises the CollectionChanged event with the specified event arguments
	/// </summary>
	public void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		OnCollectionChanged(e);
	}

	/// <summary>
	/// Replaces an existing object in the collection with a new object, maintaining the original object's index position
	/// </summary>
	public void Replace(T oldObject, T newObject)
	{
		int index = Items.IndexOf(oldObject);
		if (index >= 0)
		{
			RemoveAt(index);
			Insert(index, newObject);
		}
		else
		{
			Add(newObject);
		}
	}
}
