using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Utilities;
using System.Collections;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Wraps an object with its ToString() representation and optional data key/value for DataGrid display
/// </summary>
[Skippable]
public class ListToString
{
	/// <summary>
	/// Gets or sets the maximum number of items to create from an enumerable (default: 200,000)
	/// </summary>
	public static int MaxItems { get; set; } = 200_000;

	/// <summary>
	/// Gets the underlying object
	/// </summary>
	[InnerValue, HiddenColumn]
	public object Object { get; }

	/// <summary>
	/// Gets or sets the string representation of the object
	/// </summary>
	public string? Value { get; set; }

	/// <summary>
	/// Gets or sets the data key for binding
	/// </summary>
	[DataKey, HiddenColumn]
	public string? DataKey { get; set; }

	/// <summary>
	/// Gets or sets the data value for binding
	/// </summary>
	[DataValue, HiddenColumn]
	public object? DataValue { get; set; }

	public override string? ToString() => Value;

	/// <summary>
	/// Initializes a new ListToString wrapper for the specified object
	/// </summary>
	public ListToString(object obj)
	{
		Object = obj;
		if (obj == null)
			return;

		Value = obj.ToString();

		DataKey = ObjectUtils.GetDataKey(obj);
		DataValue = ObjectUtils.GetDataValue(obj);
	}

	/// <summary>
	/// Creates a collection of ListToString items from an enumerable, limited to the specified maximum
	/// </summary>
	/// <param name="enumerable">The enumerable to convert</param>
	/// <param name="limit">Maximum number of items to create (uses MaxItems if not specified)</param>
	public static ItemCollection<ListToString> Create(IEnumerable enumerable, int? limit = null)
	{
		limit ??= MaxItems;

		var list = new ItemCollection<ListToString>();
		if (enumerable is IItemCollection sourceCollection)
		{
			(list as IItemCollection).LoadSettings(sourceCollection);
		}
		foreach (object obj in enumerable)
		{
			list.Add(new ListToString(obj));
			if (list.Count > limit)
				break;
		}
		return list;
	}
}
