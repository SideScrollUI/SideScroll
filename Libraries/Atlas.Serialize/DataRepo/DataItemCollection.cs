using Atlas.Core;
using System.Reflection;

namespace Atlas.Serialize;

// Collection of DataRepo items with a key/value lookup
public class DataItemCollection<T> : ItemCollection<DataItem<T>>
{
	public SortedDictionary<string, T> Lookup { get; set; } = new();

	public IEnumerable<T> Values => this.Select(o => o.Value);
	public IEnumerable<T> SortedValues => Lookup.Values.Select(o => o);

	public DataItemCollection()	{ }

	// Don't implement List<T>, it isn't sortable
	public DataItemCollection(IEnumerable<DataItem<T>> iEnumerable) : base(iEnumerable)
	{
		Lookup = CreateLookup();
	}

	private SortedDictionary<string, T> CreateLookup()
	{
		var entries = new SortedDictionary<string, T>();
		foreach (DataItem<T> item in ToList())
		{
			entries.Add(item.Key, item.Value);
		}
		return entries;
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
	public object Object => Value!;
	public string? Path { get; set; }

	public FileInfo? FileInfo => _fileInfo ??= File.Exists(Path) ? new FileInfo(Path) : null;
	private FileInfo? _fileInfo;

	public DateTime? ModifiedUtc => FileInfo?.LastWriteTimeUtc;

	public override string ToString() => Key;

	public DataItem(string key, T value, string? path = null)
	{
		Key = key;
		Value = value;
		Path = path;
	}
}
