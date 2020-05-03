using Atlas.Extensions;
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

	// subclass ItemCollection? NamedItemCollection?
	public interface INamedItemCollection
	{
		string ColumnName { get; set; }
	}

	// Winforms really need IBindingList, but Wpf DataGrid tries to use IBindingList to sort if available (bad)
	// Would be nice to make this thread safe to make storing logs easier?
	public class ItemCollection<T> : ObservableCollection<T>, IList, INamedItemCollection, ICollection, IEnumerable, IComparer //, IRaiseItemChangedEvents //
	{
		public string ColumnName { get; set; }
		public string Label { get; set; }

		private CustomComparer customComparer = new CustomComparer();

		public ItemCollection()
		{
		}

		public ItemCollection(string columnName)
		{
			ColumnName = columnName;
		}

		public override string ToString() => Label ?? "[" + Count.ToString("N0") + "]";

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

	public class ItemCollectionUI<T> : ObservableCollection<T>, IList, ICollection, IEnumerable, IContext //, IRaiseItemChangedEvents //
	{
		public SynchronizationContext Context { get; set; } // inherited from creator (which can be a Parent Log)

		public ItemCollectionUI()
		{
			InitializeContext();
		}

		// Don't implement List<T>, it isn't sortable
		public ItemCollectionUI(IEnumerable<T> iEnumerable) :
			base(iEnumerable)
		{
			InitializeContext();
		}

		public void InitializeContext(bool reset = false)
		{
			if (Context == null || reset)
			{
				Context = SynchronizationContext.Current;
				if (Context == null)
				{
					//contextRandomId = new Random().Next();
					//throw new Exception("Don't do this");
					Context = new SynchronizationContext();
				}
			}
		}

		struct ItemLocation
		{
			public int index;
			public T item;

			public ItemLocation(int index, T item)
			{
				this.index = index;
				this.item = item;
			}
		}

		protected override void InsertItem(int index, T item)
		{
			var location = new ItemLocation(index, item);
			if (Context == SynchronizationContext.Current)
				InsertItemCallback(location);
			else
				Context.Post(new SendOrPostCallback(InsertItemCallback), location); // inserting 2 items inserts in wrong order
		}

		// Thread safe callback, only works if the context is the same
		private void InsertItemCallback(object state)
		{
			ItemLocation itemLocation = (ItemLocation)state;
			lock (Context)
			{
				base.InsertItem(itemLocation.index, itemLocation.item);
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
