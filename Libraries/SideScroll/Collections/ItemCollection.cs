using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SideScroll.Collections;

public interface IItemCollection
{
	string? ColumnName { get; set; }
	string? CustomSettingsPath { get; set; }
	public object? DefaultSelectedItem { get; set; }
	bool Skippable { get; set; }
	bool? ShowHeader { get; set; } // Override default visibility and always show

	void LoadSettings(IItemCollection itemCollection)
	{
		ColumnName = itemCollection.ColumnName;
		CustomSettingsPath = itemCollection.CustomSettingsPath;
		DefaultSelectedItem = itemCollection.DefaultSelectedItem;
		Skippable = itemCollection.Skippable;
		ShowHeader = itemCollection.ShowHeader;
	}
}

// See ItemCollectionUI for a UI thread safe version
public class ItemCollection<T> : ObservableCollection<T>, IItemCollection, IComparer //, IRaiseItemChangedEvents //
{
	public string? Label { get; set; }
	public string? ColumnName { get; set; }
	public string? CustomSettingsPath { get; set; }
	public object? DefaultSelectedItem { get; set; }
	public bool Skippable { get; set; } = true;
	public bool? ShowHeader { get; set; }

	public IComparer Comparer { get; set; } = new CustomComparer();

	public override string ToString() => Label ?? "[" + Count.ToString("N0") + "]";

	public ItemCollection() { }

	public ItemCollection(string columnName)
	{
		ColumnName = columnName;
	}

	// Don't implement List<T>, it isn't sortable
	public ItemCollection(IEnumerable<T> enumerable) :
		base(enumerable)
	{
	}

	public void AddRange(IEnumerable<T> collection)
	{
		foreach (T item in collection)
			Items.Add(item);

		// DataGrid takes too long to load these using Add
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	public int Compare(object? x, object? y)
	{
		int result = Comparer.Compare(x, y);
		return result;
	}

	public ItemCollection<T> FilterNull()
	{
		var filtered = ToList().Where(i => i != null);
		return new ItemCollection<T>(filtered);
	}

	public List<T> ToList()
	{
		return new List<T>(this);
	}

	/*public new void Add(T item)
	{
		base.Add(item);
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
	}*/
}

public class ItemQueueCollection<T> : ItemCollection<T>
{
	public int MaxCount { get; set; } = 100;

	public new void Add(T item)
	{
		base.Add(item);
		if (Count > MaxCount)
			RemoveAt(0);
	}
}
