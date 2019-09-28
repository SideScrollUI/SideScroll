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
				this.RemoveAt(0);
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

		CustomComparer customComparer = new CustomComparer();

		public string ColumnName { get; set; }

		public int Compare(object x, object y)
		{
			int result = customComparer.Compare(x, y);
			return result;
		}

		public void AddRange(IEnumerable<T> collection)
		{
			foreach (T item in collection)
				Items.Add(item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
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

	public class ThreadedItemCollection<T> : ObservableCollection<T>, IList, ICollection, IEnumerable //, IRaiseItemChangedEvents //
	{
		private SynchronizationContext context; // inherited from creator (which can be a Parent Log)

		public ThreadedItemCollection()
		{
			InitializeContext();
		}

		// Don't implement List<T>, it isn't sortable
		public ThreadedItemCollection(IEnumerable<T> iEnumerable) :
			base(iEnumerable)
		{
			InitializeContext();
		}

		private void InitializeContext()
		{
			if (this.context == null)
			{
				this.context = SynchronizationContext.Current;
				if (this.context == null)
				{
					//contextRandomId = new Random().Next();
					//throw new Exception("Don't do this");
					this.context = new SynchronizationContext();
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
			if (context == SynchronizationContext.Current)
				InsertItemCallback(location);
			else
				context.Post(new SendOrPostCallback(this.InsertItemCallback), location); // inserting 2 items inserts in wrong order
		}

		// Thread safe callback, only works if the context is the same
		private void InsertItemCallback(object state)
		{
			ItemLocation itemLocation = (ItemLocation)state;
			lock (context)
			{
				base.InsertItem(itemLocation.index, itemLocation.item);
			}
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
