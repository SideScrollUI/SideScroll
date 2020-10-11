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
	}

	// Winforms really need IBindingList, but Wpf DataGrid tries to use IBindingList to sort if available (bad)
	// Would be nice to make this thread safe to make storing logs easier?
	public class ItemCollection<T> : ObservableCollection<T>, IList, IItemCollection, ICollection, IEnumerable, IComparer //, IRaiseItemChangedEvents //
	{
		public string ColumnName { get; set; }
		public string Label { get; set; }
		public bool Skippable { get; set; } = true;

		private CustomComparer customComparer = new CustomComparer();

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
			int result = customComparer.Compare(x, y);
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

	public interface IContext
	{
		SynchronizationContext Context { get; set; }
		void InitializeContext(bool reset = false);
	}

	public class ItemCollectionUI<T> : ObservableCollection<T>, IList, IItemCollection, ICollection, IEnumerable, IContext //, IRaiseItemChangedEvents //
	{
		public string ColumnName { get; set; }
		public string Label { get; set; }
		public bool Skippable { get; set; } = true;
		public SynchronizationContext Context { get; set; } // TabInstance will initialize this, don't want to initialize this early due to default SynchronizationContext not posting messages in order

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

		struct ItemLocation
		{
			public int Index;
			public T Item;

			public ItemLocation(int index, T item)
			{
				Index = index;
				Item = item;
			}
		}

		protected override void InsertItem(int index, T item)
		{
			var location = new ItemLocation(index, item);
			if (Context == null)
				base.InsertItem(index, item);
			else if (Context == SynchronizationContext.Current)
				InsertItemCallback(location);
			else
				Context.Post(new SendOrPostCallback(InsertItemCallback), location); // default context inserts multiple items in wrong order, AvaloniaUI doesn't
		}

		// Thread safe callback, only works if the context is the same
		private void InsertItemCallback(object state)
		{
			ItemLocation itemLocation = (ItemLocation)state;
			lock (Context)
			{
				base.InsertItem(itemLocation.Index, itemLocation.Item);
			}
		}

		protected override void RemoveItem(int index)
		{
			if (Context == null)
				base.RemoveItem(index);
			else if (Context == SynchronizationContext.Current)
				RemoveItemCallback(index);
			else
				Context.Post(new SendOrPostCallback(RemoveItemCallback), index);
		}

		// Thread safe callback, only works if the context is the same
		private void RemoveItemCallback(object state)
		{
			int index = (int)state;
			lock (Context)
			{
				base.RemoveItem(index);
			}
		}

		public void AddRange(IEnumerable<T> collection)
		{
			int index = Items.Count;
			foreach (T item in collection)
				InsertItem(index++, item); // item gets added in the background with Add() and doesn't increment index

			//foreach (T item in collection)
			//	Items.Add(item);
			//OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)); // need ui thread
		}
	}


	// Winforms really need IBindingList, but Wpf DataGrid tries to use IBindingList to sort if available (bad)
	// Would be nice to make this thread safe to make storing logs easier?
	/*public class ItemCollectionView<T> : ItemCollection<T> //, IRaiseItemChangedEvents //
	{
		private ItemCollection<T> itemCollection;

		public ItemCollectionView(ItemCollection<T> itemCollection)
		{
			this.itemCollection = itemCollection;
		}

		// Don't implement List<T>, it isn't sortable
		public ItemCollectionView(IEnumerable<T> iEnumerable) :
			base(iEnumerable)
		{

		}
	}*/
}
