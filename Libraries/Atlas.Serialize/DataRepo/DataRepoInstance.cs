using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Serialize
{
	public interface IDataRepoInstance
	{
		string GroupId { get; }
		//object GetObject(string key);
	}

	public class DataRepoInstance<T> : IDataRepoInstance
	{
		private const string DefaultKey = ".Default"; // todo: support multiple directory levels?

		public DataRepo DataRepo;
		public string GroupId { get; set; }

		public DataRepoInstance(DataRepo dataRepo, string groupId)
		{
			DataRepo = dataRepo;
			GroupId = groupId;
		}

		public virtual void Save(Call call, T item)
		{
			DataRepo.Save<T>(GroupId, DefaultKey, item, call);
		}

		public virtual void Save(Call call, string key, T item)
		{
			DataRepo.Save<T>(GroupId, key, item, call);
		}

		public virtual T Load(Call call, string key = null, bool createIfNeeded = false, bool lazy = false)
		{
			return DataRepo.Load<T>(GroupId, key ?? DefaultKey, call, createIfNeeded, lazy);
		}

		public DataItemCollection<T> LoadAll(Call call = null, bool lazy = false)
		{
			return DataRepo.LoadAll<T>(call, GroupId, lazy);
		}

		public SortedDictionary<string, T> LoadAllSorted(Call call = null, bool lazy = false)
		{
			return DataRepo.LoadAllSorted<T>(call, GroupId, lazy);
		}

		public virtual void Delete(string key = null)
		{
			DataRepo.Delete<T>(GroupId, key ?? DefaultKey);
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

		public DataRepoView(DataRepo dataRepo, string groupId) : base(dataRepo, groupId)
		{
			Initialize();
		}

		public DataRepoView(DataRepoInstance<T> dataRepo) : base(dataRepo.DataRepo, dataRepo.GroupId)
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
}
