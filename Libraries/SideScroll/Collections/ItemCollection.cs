using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SideScroll.Collections;

/// <summary>
/// Interface for collections that can be displayed in DataGrids with customizable display settings
/// </summary>
public interface IItemCollection
{
	/// <summary>
	/// The name to display for the column in the DataGrid
	/// </summary>
	string? ColumnName { get; set; }
	
	/// <summary>
	/// Custom path for saving/loading tab view settings
	/// </summary>
	string? CustomSettingsPath { get; set; }
	
	/// <summary>
	/// The default item to select when the collection is displayed
	/// </summary>
	public object? DefaultSelectedItem { get; set; }
	
	/// <summary>
	/// Whether single-item collections should be automatically skipped/expanded in the UI
	/// </summary>
	bool Skippable { get; set; }
	
	/// <summary>
	/// Override default header visibility - when set, always shows/hides the header regardless of other settings
	/// </summary>
	bool? ShowHeader { get; set; }

	/// <summary>
	/// Loads display settings from another IItemCollection
	/// </summary>
	void LoadSettings(IItemCollection itemCollection)
	{
		ColumnName = itemCollection.ColumnName;
		CustomSettingsPath = itemCollection.CustomSettingsPath;
		DefaultSelectedItem = itemCollection.DefaultSelectedItem;
		Skippable = itemCollection.Skippable;
		ShowHeader = itemCollection.ShowHeader;
	}
}

/// <summary>
/// An observable collection for displaying items in DataGrids. For thread-safe UI updates, use ItemCollectionUI instead.
/// Provides customizable display settings and supports sorting via IComparer.
/// </summary>
public class ItemCollection<T> : ObservableCollection<T>, IItemCollection, IComparer //, IRaiseItemChangedEvents //
{
	/// <summary>
	/// The label to display for this collection
	/// </summary>
	public string? Label { get; set; }
	
	/// <summary>
	/// The name to display for the column in the DataGrid
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
	/// The comparer used for sorting items in the collection
	/// </summary>
	public IComparer Comparer { get; set; } = new CustomComparer();

	public override string ToString() => Label ?? "[" + Count.ToString("N0") + "]";

	public ItemCollection() { }

	/// <summary>
	/// Initializes a new ItemCollection with the specified column name
	/// </summary>
	public ItemCollection(string columnName)
	{
		ColumnName = columnName;
	}

	/// <summary>
	/// Initializes a new ItemCollection with items from an enumerable collection
	/// </summary>
	public ItemCollection(IEnumerable<T> enumerable) :
		base(enumerable)
	{
	}

	/// <summary>
	/// Adds multiple items to the collection efficiently with a single collection changed notification
	/// </summary>
	public virtual void AddRange(IEnumerable<T> collection)
	{
		foreach (T item in collection)
		{
			Items.Add(item);
		}

		// DataGrid takes too long to load these using Add
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	/// <summary>
	/// Compares two objects using the collection's Comparer
	/// </summary>
	public int Compare(object? x, object? y)
	{
		int result = Comparer.Compare(x, y);
		return result;
	}

	/// <summary>
	/// Returns a new ItemCollection containing only non-null items
	/// </summary>
	public ItemCollection<T> FilterNull()
	{
		var filtered = ToList().Where(i => i != null);
		return new ItemCollection<T>(filtered);
	}

	/// <summary>
	/// Converts the collection to a List
	/// </summary>
	public List<T> ToList()
	{
		return [.. this];
	}

	/*public new void Add(T item)
	{
		base.Add(item);
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
	}*/
}

/// <summary>
/// A queue-based collection that automatically removes the oldest item when MaxCount is exceeded
/// </summary>
public class ItemQueueCollection<T> : ItemCollection<T>
{
	/// <summary>
	/// The maximum number of items to keep in the collection (default: 100)
	/// </summary>
	public int MaxCount { get; set; } = 100;

	/// <summary>
	/// Adds an item to the collection, removing the oldest item if MaxCount is exceeded
	/// </summary>
	public new void Add(T item)
	{
		base.Add(item);
		if (Count > MaxCount)
		{
			RemoveAt(0);
		}
	}
}
