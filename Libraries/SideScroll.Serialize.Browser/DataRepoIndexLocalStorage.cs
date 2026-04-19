using System.Runtime.Versioning;
using System.Text.Json;
using SideScroll.Serialize.Atlas;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Serialize.Browser;

/// <summary>
/// localStorage-based index for browser applications
/// Stores index data in localStorage instead of filesystem
/// </summary>
[SupportedOSPlatform("browser")]
public class DataRepoIndexLocalStorage<T>(DataRepoInstance<T> dataRepoInstance, int? maxItems = null)
	: DataRepoIndex<T>(dataRepoInstance, maxItems)
{
	private string IndexStorageKey => SerializerLocalStorage.ConvertPathToStorageKey(PrimaryIndexPath);

	/// <summary>
	/// Loads the index from localStorage or builds it if it doesn't exist
	/// </summary>
	public override Indices Load(Call call)
	{
		string? json = SerializerLocalStorage.GetItem(IndexStorageKey);

		if (!string.IsNullOrEmpty(json))
		{
			try
			{
				var indices = JsonSerializer.Deserialize<Indices>(json);
				if (indices != null)
				{
					call.Log.AddDebug("Loaded index from localStorage",
						new Tag("Key", IndexStorageKey),
						new Tag("Count", indices.Items.Count));
					return indices;
				}
			}
			catch (Exception ex)
			{
				call.Log.Add(ex,
					new Tag("Key", IndexStorageKey),
					new Tag("Operation", "LoadIndex"));
			}
		}

		return BuildIndicesFromLocalStorage(call);
	}

	/// <summary>
	/// Saves the index to localStorage
	/// </summary>
	protected override void Save(Indices indices)
	{
		string json = JsonSerializer.Serialize(indices);
		SerializerLocalStorage.SetItem(IndexStorageKey, json);
	}

	/// <summary>
	/// Saves an item to the index
	/// </summary>
	public override Item? Save(Call call, string key)
	{
		// Use a simple lock instead of mutex (not available in browser)
		lock (DataRepoInstance)
		{
			return SaveIndexItem(call, key);
		}
	}

	private Item SaveIndexItem(Call call, string key)
	{
		Indices indices = Load(call);

		if (indices.Items.FirstOrDefault(item => item.Key == key) is Item existingItem)
		{
			return existingItem;
		}

		long index = indices.NextIndex++;
		Item newItem = new(index, key);
		indices.Items.Add(newItem);
		Save(indices);

		PruneMaxItemsFromLocalStorage(call, indices);

		return newItem;
	}

	private void PruneMaxItemsFromLocalStorage(Call call, Indices indices)
	{
		if (MaxItems is not int maxItems) return;

		while (indices.Items.Count > maxItems)
		{
			DataRepoInstance.Delete(call, indices.Items[0].Key);
			indices.Items.RemoveAt(0);
			Save(indices);
		}
	}

	/// <summary>
	/// Removes an item from the index by key
	/// </summary>
	public override void Remove(Call call, string key)
	{
		lock (DataRepoInstance)
		{
			RemoveIndexItem(call, key);
		}
	}

	private void RemoveIndexItem(Call call, string key)
	{
		Indices indices = Load(call);
		int removed = indices.Items.RemoveAll(item => item.Key == key);
		if (removed > 0)
		{
			Save(indices);
		}
	}

	/// <summary>
	/// Removes all items from the index
	/// </summary>
	public override void RemoveAll(Call call)
	{
		lock (DataRepoInstance)
		{
			RemoveAllIndexItems(call);
		}
	}

	private void RemoveAllIndexItems(Call call)
	{
		Indices indices = Load(call);
		indices.Items.Clear();
		Save(indices);
	}

	private Indices BuildIndicesFromLocalStorage(Call call)
	{
		List<SerializerHeader> headers = DataRepoInstance.LoadHeaders(call);

		int index = 0;
		List<Item> items = headers
			.Select(header => new Item(index++, header.Name ?? ""))
			.ToList();

		Indices indices = new()
		{
			Items = items,
			NextIndex = index,
		};

		// Save the newly built index
		Save(indices);

		return indices;
	}
}
