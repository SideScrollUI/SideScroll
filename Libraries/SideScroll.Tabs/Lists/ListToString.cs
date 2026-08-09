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
	/// <remarks>
	/// Kept in sync with <see cref="TabModel.MaxItems"/>, which caps the other branch
	/// </remarks>
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

	/// <summary>Returns the wrapped <see cref="Value"/>.</summary>
	public override string? ToString() => Value;

	/// <summary>
	/// Initializes a new ListToString wrapper for the specified object
	/// </summary>
	public ListToString(object obj)
	{
		Object = obj;
		if (obj == null)
			return;

		try
		{
			Value = obj.ToString();
		}
		catch (Exception e)
		{
			// Show the failure instead of propagating it. Create() builds every row from this, so a
			// single item with a throwing ToString() failed the entire collection and left the tab
			// unrendered. Value is the only visible column, so a null would show an empty row
			Value = "Exception: " + e.Message;
		}

		// A throwing [DataKey] or [DataValue] getter leaves the item unidentified rather than failing
		// the collection it belongs to. Both are best effort, matching ListProperty and ListField,
		// and are read separately so an unreadable value keeps a key that was read fine
		try
		{
			DataKey = ObjectUtils.GetDataKey(obj);
		}
		catch (Exception)
		{
		}

		try
		{
			DataValue = ObjectUtils.GetDataValue(obj);
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// Creates a collection of ListToString items from an enumerable, limited to the specified maximum
	/// </summary>
	/// <param name="enumerable">The enumerable to convert</param>
	/// <param name="limit">Maximum number of items to create, zero or less creates none (uses MaxItems if not specified)</param>
	public static ItemCollection<ListToString> Create(IEnumerable enumerable, int? limit = null)
	{
		limit ??= MaxItems;

		var list = new ItemCollection<ListToString>();
		if (enumerable is IItemCollection sourceCollection)
		{
			(list as IItemCollection).LoadSettings(sourceCollection);
		}
		// Check before adding so a limit of 0 (or a negative one) doesn't still add the first item
		foreach (object obj in enumerable)
		{
			if (list.Count >= limit)
				break;

			list.Add(new ListToString(obj));
		}
		return list;
	}
}
