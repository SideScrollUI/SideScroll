using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Atlas.Core;

public class ItemQueueCollection<T> : ItemCollection<T>
{
	public new void Add(T item)
	{
		base.Add(item);
		if (Count > 100)
			RemoveAt(0);
	}
}

public interface IItemCollection
{
	string ColumnName { get; set; }
	bool Skippable { get; set; }
	string CustomSettingsPath { get; set; }
}

// See ItemCollectionUI for a thread safe version
public class ItemCollection<T> : ObservableCollection<T>, IItemCollection, IComparer //, IRaiseItemChangedEvents //
{
	public string ColumnName { get; set; }
	public string Label { get; set; }
	public bool Skippable { get; set; } = true;
	public string CustomSettingsPath { get; set; }

	public IComparer Comparer { get; set; } = new CustomComparer();

	public override string ToString() => Label ?? "[" + Count.ToString("N0") + "]";

	public ItemCollection()	{ }

	public ItemCollection(string columnName)
	{
		ColumnName = columnName;
	}

	// Don't implement List<T>, it isn't sortable
	public ItemCollection(IEnumerable<T> iEnumerable) :
		base(iEnumerable)
	{
	}

	public void AddRange(IEnumerable<T> collection)
	{
		foreach (T item in collection)
			Items.Add(item);
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	public int Compare(object x, object y)
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

public class ItemCollection<T, T2> : ObservableCollection<T>, IList, ICollection, IEnumerable //, IRaiseItemChangedEvents //
{
	public ItemCollection()
	{
	}

	// Don't implement List<T>, it isn't sortable
	public ItemCollection(IEnumerable<T> iEnumerable) :
		base(iEnumerable)
	{

	}
}
