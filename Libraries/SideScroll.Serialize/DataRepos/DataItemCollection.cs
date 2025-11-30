using SideScroll.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SideScroll.Serialize.DataRepos;

// Collection of DataRepo items with a key/value lookup
public class DataItemCollection<T> : ItemCollection<DataItem<T>>
{
	public SortedDictionary<string, DataItem<T>> Lookup { get; } = [];

	public IEnumerable<string> Keys => this.Select(o => o.Key);
	public IEnumerable<T> Values => this.Select(o => o.Value);
	public IEnumerable<T> SortedValues => Lookup.Values.Select(o => o.Value);

	public DataItemCollection() { }

	// Don't implement List<T>, it isn't sortable
	public DataItemCollection(IEnumerable<DataItem<T>> enumerable) : base(enumerable)
	{
	}

	public IEnumerable<DataItem<T>> OrderBy(string propertyName)
	{
		PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName)!;

		return ToList().OrderBy(i => propertyInfo.GetValue(i.Value));
	}

	public IEnumerable<DataItem<T>> OrderByDescending(string propertyName)
	{
		PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName)!;

		return ToList().OrderByDescending(i => propertyInfo.GetValue(i.Value));
	}

	public void Add(string key, T value)
	{
		var dataItem = new DataItem<T>(key, value);
		Add(dataItem);
	}

	public new void Add(DataItem<T> dataItem)
	{
		base.Add(dataItem);
		Lookup.Add(dataItem.Key, dataItem);
	}

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

	public bool ContainsKey(string key) => Lookup.ContainsKey(key);

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

	public new void Remove(DataItem<T> dataItem)
	{
		base.Remove(dataItem);
		Lookup.Remove(dataItem.Key);
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

public class DataItem<T>(string key, T value, string? path = null) : IDataItem
{
	public string Key { get; } = key;
	public T Value { get; set; } = value;
	public object Object => Value!;
	public string? Path { get; set; } = path;

	public FileInfo? FileInfo => _fileInfo ??= File.Exists(Path) ? new FileInfo(Path) : null;
	private FileInfo? _fileInfo;

	public DateTime? ModifiedUtc => FileInfo?.LastWriteTimeUtc;

	public override string ToString() => Key;
}
