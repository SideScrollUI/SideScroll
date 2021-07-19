using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

namespace Atlas.Core
{
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

	// Winforms really need IBindingList, but Wpf DataGrid tries to use IBindingList to sort if available (bad)
	// Would be nice to make this thread safe to make storing logs easier?
	public class ItemCollection<T> : ObservableCollection<T>, IList, IItemCollection, ICollection, IEnumerable, IComparer //, IRaiseItemChangedEvents //
	{
		public string ColumnName { get; set; }
		public string Label { get; set; }
		public bool Skippable { get; set; } = true;
		public string CustomSettingsPath { get; set; }

		public IComparer Comparer = new CustomComparer();

		public override string ToString() => Label ?? "[" + Count.ToString("N0") + "]";

		public ItemCollection()
		{
		}

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

		public List<T> ToList()
		{
			var list = new List<T>();
			foreach (T item in this)
				list.Add(item);
			return list;
		}

		/*public new void Add(T item)
		{
			base.Add(item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
		}*/
	}

	// Winforms really need IBindingList, but Wpf DataGrid tries to use IBindingList to sort if available (bad)
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
}
