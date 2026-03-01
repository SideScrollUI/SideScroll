using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Utilities;

namespace SideScroll.Tabs.Bookmarks.Models;

/// <summary>
/// Represents a selected row in a data grid with label, index, and optional data key/value for bookmark persistence
/// </summary>
[PublicData]
public class SelectedRow : IEquatable<SelectedRow>
{
	/// <summary>
	/// Gets or sets the display label (ToString() value, can be null)
	/// </summary>
	public string? Label { get; set; }

	/// <summary>
	/// Gets or sets the zero-based row index in the original unfiltered list
	/// </summary>
	public int? RowIndex { get; set; }

	/// <summary>
	/// Gets or sets the underlying object (used for bookmark searches, not serialized)
	/// </summary>
	[Unserialized]
	public object? Object { get; set; }

	/// <summary>
	/// Gets or sets the data key for identifying this row
	/// </summary>
	public string? DataKey { get; set; }

	/// <summary>
	/// Gets or sets the data value (imported with bookmark into the app DataRepo)
	/// </summary>
	public object? DataValue { get; set; }

	public override string? ToString() => DataKey ?? Label;

	/// <summary>
	/// Initializes a new empty selected row
	/// </summary>
	public SelectedRow() { }

	/// <summary>
	/// Initializes a new selected row from an object, extracting its label and data key/value
	/// </summary>
	public SelectedRow(object obj)
	{
		if (obj == null) return; // obj can still be null
		Object = obj;

		Label = obj.ToString();

		DataKey = ObjectUtils.GetDataKey(obj); // overrides label
		DataValue = ObjectUtils.GetDataValue(obj);

		// Use the DataValue's DataKey if no DataKey found
		if (DataKey == null && DataValue != null)
		{
			DataKey = ObjectUtils.GetDataKey(DataValue);
		}

		Type type = obj.GetType();
		if (Label == type.GetAssemblyQualifiedShortName())
		{
			Label = null;
		}
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SelectedRow);
	}

	/// <summary>
	/// Determines whether this selected row equals another by comparing label, data key, data value, and optionally row index
	/// </summary>
	public bool Equals(SelectedRow? other)
	{
		return other != null &&
			   Label == other.Label &&
			   DataKey == other.DataKey &&
			   DataValue == other.DataValue &&
			   (RowIndex == other.RowIndex || RowIndex == null || other.RowIndex == null); // Allow matching on missing rows
	}

	public override int GetHashCode()
	{
		return (Label?.GetHashCode() ?? 0)
			^ (DataKey?.GetHashCode() ?? 0)
			^ (DataValue?.GetHashCode() ?? 0)
			^ (RowIndex?.GetHashCode() ?? 0);
	}
}
