using SideScroll.Serialize.Atlas;
using SideScroll.Tasks;

namespace SideScroll.Serialize.DataRepos;

public class DataRepoIndex<T>(DataRepoInstance<T> dataRepoInstance, int? maxItems = null)
{
	public static TimeSpan MutexTimeout { get; set; } = TimeSpan.FromSeconds(5);

	public DataRepoInstance<T> DataRepoInstance => dataRepoInstance;

	public int? MaxItems { get; set; } = maxItems;

	public string GroupId => DataRepoInstance.GroupId;
	public string GroupPath => DataRepoInstance.GroupPath;

	public string IndexPath => Paths.Combine(GroupPath, "Index.dat");

	// Don't use GroupId since it can throw exceptions due to invalid characters
	protected string MutexName => DataRepoInstance.GroupHash;

	public record Item(long Index, string Key);

	public class Indices
	{
		public List<Item> Items { get; set; } = [];
		public long NextIndex { get; set; }
	}

	public Item? Save(Call call, string key)
	{
		return LockedGetCall(call, () => SaveInternal(call, key));
	}

	private Item SaveInternal(Call call, string key)
	{
		Indices indices = Load(call);

		if (indices.Items.FirstOrDefault(item => item.Key == key) is Item existingItem)
		{
			return existingItem;
		}
		else
		{
			long index = indices.NextIndex++;
			Item newItem = new(index, key);
			indices.Items.Add(newItem);
			Save(indices);

			PruneMaxItems(call, indices);

			return newItem;
		}
	}

	private void PruneMaxItems(Call call, Indices indices)
	{
		if (MaxItems is int maxItems)
		{
			while (indices.Items.Count > maxItems)
			{
				DataRepoInstance.Delete(call, indices.Items[0].Key);
				indices.Items.RemoveAt(0);
				Save(indices);
			}
		}
	}

	public void Remove(Call call, string key)
	{
		LockedSetCall(call, (c) => RemoveInternal(c, key));
	}

	private void RemoveInternal(Call call, string key)
	{
		Indices indices = Load(call);
		indices.Items.RemoveAll(item => item.Key == key);
		Save(indices);
	}

	public void RemoveAll(Call call)
	{
		LockedSetCall(call, RemoveAllInternal);
	}

	private void RemoveAllInternal(Call call)
	{
		Indices indices = Load(call);
		indices.Items.Clear();
		Save(indices);
	}

	public TResult? LockedGetCall<TResult>(Call call, Func<TResult> func)
	{
		using var mutex = new Mutex(false, MutexName);

		try
		{
			if (!mutex.WaitOne(MutexTimeout))
			{
				throw new TaggedException("Index timed out waiting for mutex",
					new Tag("Timeout", MutexTimeout),
					new Tag("Function", func));
			}
		}
		catch (AbandonedMutexException e)
		{
			// Mutex acquired
			call.Log.Add(e);
		}
		catch (Exception e)
		{
			call.Log.Throw(e);
		}

		try
		{
			// Do operation
			TResult result = func();
			return result;
		}
		catch (ApplicationException e)
		{
			call.Log.Add(e);
		}
		finally
		{
			mutex.ReleaseMutex();
		}
		return default;
	}

	public void LockedSetCall(Call call, CallAction callAction)
	{
		using var mutex = new Mutex(false, MutexName);

		try
		{
			if (!mutex.WaitOne(MutexTimeout))
			{
				throw new TaggedException("Index timed out waiting for mutex",
					new Tag("Timeout", MutexTimeout),
					new Tag("Action", callAction));
			}
		}
		catch (AbandonedMutexException e)
		{
			// Mutex acquired
			call.Log.Add(e);
		}
		catch (Exception e)
		{
			call.Log.Throw(e);
		}

		try
		{
			// Do operation
			callAction(call);
		}
		catch (ApplicationException e)
		{
			call.Log.Add(e);
		}
		finally
		{
			mutex.ReleaseMutex();
		}
	}

	private void Save(Indices indices)
	{
		if (!Directory.Exists(GroupPath))
		{
			Directory.CreateDirectory(GroupPath);
		}
		// Don't allow reading until finished since we seek backwards at the end to set the file size
		// FileShare.None also avoids simultaneous writes
		using var stream = new FileStream(IndexPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		using var writer = new BinaryWriter(stream);

		writer.Write(indices.Items.Count);
		writer.Write(indices.NextIndex);

		foreach (Item item in indices.Items)
		{
			writer.Write(item.Index);
			writer.Write(item.Key);
		}
	}

	public Indices Load(Call call)
	{
		if (!File.Exists(IndexPath)) return BuildIndices(call);

		using var stream = new FileStream(IndexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using var reader = new BinaryReader(stream);

		List<Item> items = [];
		int count = reader.ReadInt32();
		long nextIndex = reader.ReadInt64();
		for (int i = 0; i < count; i++)
		{
			long index = reader.ReadInt64();
			string key = reader.ReadString();
			if (index > nextIndex)
			{
				call.Log.AddWarning("Index > NextIndex",
					new Tag("Index", index),
					new Tag("Key", key));
				nextIndex = index + 1;
			}
			items.Add(new Item(index, key));
		}
		return new Indices()
		{
			Items = items,
			NextIndex = nextIndex,
		};
	}

	private Indices BuildIndices(Call call)
	{
		List<SerializerHeader> headers = DataRepoInstance.LoadHeaders(call);

		int index = 0;
		List<Item> items = headers
			.Select(header => new Item(index++, header.Name ?? ""))
			.ToList();

		return new Indices()
		{
			Items = items,
			NextIndex = index,
		};
	}
}
