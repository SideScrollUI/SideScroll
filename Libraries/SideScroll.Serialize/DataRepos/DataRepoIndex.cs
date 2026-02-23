using SideScroll.Serialize.Atlas;
using SideScroll.Tasks;

namespace SideScroll.Serialize.DataRepos;

/// <summary>
/// Manages an index for a data repository to maintain ordered access to items
/// </summary>
public class DataRepoIndex<T>(DataRepoInstance<T> dataRepoInstance, int? maxItems = null)
{
	/// <summary>
	/// Gets or sets the timeout duration for acquiring the mutex lock
	/// </summary>
	public static TimeSpan MutexTimeout { get; set; } = TimeSpan.FromSeconds(5);

	/// <summary>
	/// Gets the associated data repository instance
	/// </summary>
	public DataRepoInstance<T> DataRepoInstance => dataRepoInstance;

	/// <summary>
	/// Gets or sets the maximum number of items to retain in the index
	/// </summary>
	public int? MaxItems { get; set; } = maxItems;

	/// <summary>
	/// Gets the group identifier
	/// </summary>
	public string GroupId => DataRepoInstance.GroupId;
	
	/// <summary>
	/// Gets the group path
	/// </summary>
	public string GroupPath => DataRepoInstance.GroupPath;

	/// <summary>
	/// Gets the legacy index file path
	/// </summary>
	protected string OldIndexPath => Paths.Combine(GroupPath, "Index.dat");
	
	/// <summary>
	/// Gets the primary index file path
	/// </summary>
	public string PrimaryIndexPath => Paths.Combine(GroupPath, "Primary.sidx");

	/// <summary>
	/// Gets the mutex name used for thread synchronization
	/// </summary>
	protected string MutexName => DataRepoInstance.GroupHash; // GroupId can throw exceptions due to invalid characters

	/// <summary>
	/// Represents an indexed item with an index and key
	/// </summary>
	public record Item(long Index, string Key);

	/// <summary>
	/// Contains the collection of indexed items and the next index value
	/// </summary>
	public class Indices
	{
		/// <summary>
		/// Gets or sets the list of indexed items
		/// </summary>
		public List<Item> Items { get; set; } = [];
		
		/// <summary>
		/// Gets or sets the next index value to assign
		/// </summary>
		public long NextIndex { get; set; }
	}

	/// <summary>
	/// Saves an item to the index, returning the index item
	/// </summary>
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

	/// <summary>
	/// Removes an item from the index by key
	/// </summary>
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

	/// <summary>
	/// Removes all items from the index
	/// </summary>
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

	/// <summary>
	/// Executes a function with a mutex lock and returns the result
	/// </summary>
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

	/// <summary>
	/// Executes an action with a mutex lock
	/// </summary>
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
		using var stream = new FileStream(PrimaryIndexPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		using var writer = new BinaryWriter(stream);

		writer.Write(indices.Items.Count);
		writer.Write(indices.NextIndex);

		foreach (Item item in indices.Items)
		{
			writer.Write(item.Index);
			writer.Write(item.Key);
		}
	}

	/// <summary>
	/// Loads the index from disk or builds it if it doesn't exist
	/// </summary>
	public Indices Load(Call call)
	{
		if (File.Exists(PrimaryIndexPath))
		{
		}
		else if (File.Exists(OldIndexPath))
		{
			File.Move(OldIndexPath, PrimaryIndexPath);
		}
		else
		{
			return BuildIndices(call);
		}

		using var stream = new FileStream(PrimaryIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
		return new Indices
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

		return new Indices
		{
			Items = items,
			NextIndex = index,
		};
	}
}
