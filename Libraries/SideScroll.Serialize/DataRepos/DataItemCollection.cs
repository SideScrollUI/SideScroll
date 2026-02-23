using SideScroll.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SideScroll.Serialize.DataRepos;

/// <summary>
/// Collection of DataRepo items with a key/value lookup
/// </summary>
public class DataItemCollection<T> : ItemCollection<DataItem<T>>
{
	/// <summary>
	/// Gets the sorted dictionary lookup for fast key-based access
	/// </summary>
	public SortedDictionary<string, DataItem<T>> Lookup { get; } = [];

	/// <summary>
	/// Gets all keys in the collection
	/// </summary>
	public IEnumerable<string> Keys => this.Select(o => o.Key);
	
	/// <summary>
	/// Gets all values in the collection
	/// </summary>
	public IEnumerable<T> Values => this.Select(o => o.Value);
	
	/// <summary>
	/// Gets all values sorted by key
	/// </summary>
	public IEnumerable<T> SortedValues => Lookup.Values.Select(o => o.Value);

	public DataItemCollection() { }

	// Don't implement List<T>, it isn't sortable
	public DataItemCollection(IEnumerable<DataItem<T>> enumerable) : base(enumerable)
	{
	}

	/// <summary>
	/// Orders items by the specified property name in ascending order
	/// </summary>
	public IEnumerable<DataItem<T>> OrderBy(string propertyName)
	{
		PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName)!;

		return ToList().OrderBy(i => propertyInfo.GetValue(i.Value));
	}

	/// <summary>
	/// Orders items by the specified property name in descending order
	/// </summary>
	public IEnumerable<DataItem<T>> OrderByDescending(string propertyName)
	{
		PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName)!;

		return ToList().OrderByDescending(i => propertyInfo.GetValue(i.Value));
	}

	/// <summary>
	/// Adds an item with the specified key and value
	/// </summary>
	public void Add(string key, T value)
	{
		var dataItem = new DataItem<T>(key, value);
		Add(dataItem);
	}

	/// <summary>
	/// Adds a data item to the collection
	/// </summary>
	public new void Add(DataItem<T> dataItem)
	{
		base.Add(dataItem);
		Lookup.Add(dataItem.Key, dataItem);
	}

	/// <summary>
	/// Updates an existing item or adds a new one if the key doesn't exist
	/// </summary>
	public void Update(string key, T value)
	{
		if (Lookup.TryGetValue(key, out DataItem<T>? existingDataItem))
		{
			if (Equals(existingDataItem.Value, value)) return;

			int indexOfItem = IndexOf(existingDataItem);
			existingDataItem.Value = value;
			OnCollectionChanged(
				new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Replace,
					existingDataItem,
					existingDataItem,
					indexOfItem));
		}
		else
		{
			var dataItem = new DataItem<T>(key, value);
			Add(dataItem);
			Lookup[key] = dataItem;
		}
	}

	/// <summary>
	/// Determines whether the collection contains the specified key
	/// </summary>
	public bool ContainsKey(string key) => Lookup.ContainsKey(key);

	/// <summary>
	/// Attempts to get the value associated with the specified key
	/// </summary>
	public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
	{
		if (Lookup.TryGetValue(key, out DataItem<T>? lookupValue))
		{
			value = lookupValue.Value;
			return true;
		}
		value = default;
		return false;
	}

	/// <summary>
	/// Removes the specified data item from the collection
	/// </summary>
	public new void Remove(DataItem<T> dataItem)
	{
		base.Remove(dataItem);
		Lookup.Remove(dataItem.Key);
	}

	/// <summary>
	/// Removes all items from the collection
	/// </summary>
	public new void Clear()
	{
		base.Clear();
		Lookup.Clear();
	}
}

/// <summary>
/// Interface for data items with a key and object value
/// </summary>
public interface IDataItem
{
	/// <summary>
	/// Gets the unique key for this item
	/// </summary>
	string Key { get; }
	
	/// <summary>
	/// Gets the object value
	/// </summary>
	object Object { get; }
}

/// <summary>
/// Represents a key-value data item with optional file path information
/// </summary>
public class DataItem<T>(string key, T value, string? path = null) : IDataItem
{
	/// <summary>
	/// Gets the unique key for this item
	/// </summary>
	public string Key { get; } = key;
	
	/// <summary>
	/// Gets or sets the typed value
	/// </summary>
	public T Value { get; set; } = value;
	
	/// <summary>
	/// Gets the value as an object
	/// </summary>
	public object Object => Value!;
	
	/// <summary>
	/// Gets or sets the file path associated with this item
	/// </summary>
	public string? Path { get; set; } = path;

	/// <summary>
	/// Gets the file information if the path exists
	/// </summary>
	public FileInfo? FileInfo => _fileInfo ??= File.Exists(Path) ? new FileInfo(Path) : null;
	private FileInfo? _fileInfo;

	/// <summary>
	/// Gets the UTC modified date from the file information
	/// </summary>
	public DateTime? ModifiedUtc => FileInfo?.LastWriteTimeUtc;

	public override string ToString() => Key;
}
