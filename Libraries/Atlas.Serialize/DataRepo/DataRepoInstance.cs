using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.Serialize
{
	public interface IDataRepoInstance
	{
		string Directory { get; }
		//object GetObject(string key);
	}

	public class DataRepoInstance<T> : IDataRepoInstance
	{
		private const string DefaultName = ".Default"; // todo: support multiple directory levels?

		public DataRepo DataRepo;
		public string Directory { get; set; }

		public DataRepoInstance(DataRepo dataRepo, string saveDirectory)
		{
			DataRepo = dataRepo;
			Directory = saveDirectory;
		}

		public virtual void Save(Call call, T item)
		{
			DataRepo.Save<T>(Directory, DefaultName, item, call);
		}

		public virtual void Save(Call call, string key, T item)
		{
			DataRepo.Save<T>(Directory, key, item, call);
		}

		public virtual T Load(Call call, string key = null, bool createIfNeeded = false, bool lazy = false)
		{
			return DataRepo.Load<T>(Directory, key ?? DefaultName, call, createIfNeeded, lazy);
		}

		public DataItemCollection<T> LoadAll(Call call = null, bool lazy = false)
		{
			return DataRepo.LoadAll<T>(call, Directory, lazy);
		}

		public SortedDictionary<string, T> LoadAllSorted(Call call = null, bool lazy = false)
		{
			return DataRepo.LoadAllSorted<T>(call, Directory, lazy);
		}

		public virtual void Delete(string key = null)
		{
			DataRepo.Delete<T>(Directory, key ?? DefaultName);
		}

		public virtual void DeleteAll()
		{
			DataRepo.DeleteAll<T>();
		}
	}

	public class DataRepoView<T> : DataRepoInstance<T>
	{
		//public DataRepo<T> dataRepo;

		public DataItemCollection<T> Items { get; set; }

		public DataRepoView(DataRepo dataRepo, string saveDirectory) : base(dataRepo, saveDirectory)
		{
			Initialize();
		}

		public DataRepoView(DataRepoInstance<T> dataRepo) : base(dataRepo.DataRepo, dataRepo.Directory)
		{
			Initialize();
		}

		private void Initialize()
		{
			Items = LoadAll();
		}

		public override void Save(Call call, string key, T item)
		{
			Delete(key);
			base.Save(call, key, item);
			Items.Add(key, item);
		}

		public override void Delete(string key = null)
		{
			base.Delete(key);
			var item = Items.Where(d => d.Key == key).FirstOrDefault();
			if (item != null)
				Items.Remove(item);
		}

		public override void DeleteAll()
		{
			base.DeleteAll();
			Items.Clear();
		}

		public void SortBy(string memberName)
		{
			var ordered = Items.OrderBy(memberName);
			Items = new DataItemCollection<T>(ordered);
		}
	}

	public class DataItemCollection<T> : ItemCollection<DataItem<T>>
	{
		public SortedDictionary<string, T> Lookup { get; set; } = new SortedDictionary<string, T>();

		public DataItemCollection()
		{
		}

		// Don't implement List<T>, it isn't sortable
		public DataItemCollection(IEnumerable<DataItem<T>> iEnumerable) : base(iEnumerable)
		{
			Lookup = Map();
		}

		public List<T> Values => this.Select(o => o.Value).ToList();

		private SortedDictionary<string, T> Map()
		{
			var entries = new SortedDictionary<string, T>();
			foreach (DataItem<T> item in ToList())
				entries.Add(item.Key, item.Value);
			return entries;
		}

		public IEnumerable<DataItem<T>> OrderBy(string memberName)
		{
			PropertyInfo propertyInfo = typeof(T).GetProperty(memberName);
			return ToList().OrderBy(i => propertyInfo.GetValue(i.Value));
		}

		public void Add(string key, T value)
		{
			Add(new DataItem<T>(key, value));
			Lookup.Add(key, value);
		}

		public new void Remove(DataItem<T> item)
		{
			base.Remove(item);
			Lookup.Remove(item.Key);
		}

		public new void Clear()
		{
			base.Clear();
			Lookup.Clear();
		}
	}

	public interface IDataItem
	{
		string Key { get; }
		object Object { get; }
	}

	public class DataItem<T> : IDataItem
	{
		public string Key { get; set; }
		public T Value { get; set; }
		public object Object => Value;

		public DataItem()
		{
		}

		public DataItem(string key, T value)
		{
			Key = key;
			Value = value;
		}

		public override string ToString() => Key;
	}
}
