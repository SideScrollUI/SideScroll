using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Utilities;

namespace SideScroll.Tabs.Bookmarks.Models;

[PublicData]
public class SelectedRow : IEquatable<SelectedRow>
{
	public string? Label { get; set; } // ToString() value, can be null
	public int? RowIndex { get; set; } // Index in original list without filtering

	[Unserialized]
	public object? Object { get; set; } // used for bookmark searches, dangerous to keep these references around otherwise

	public string? DataKey { get; set; }

	public object? DataValue { get; set; } // Imported with bookmark into it's App DataRepo

	// public bool Pinned;
	// public List<string> SelectedColumns = []; // Not supported yet

	public override string? ToString() => DataKey ?? Label;

	public SelectedRow() { }

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
