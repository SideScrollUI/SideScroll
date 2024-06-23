using SideScroll.Utilities;

namespace SideScroll.Tabs;

[PublicData]
public class SelectedRow : IEquatable<SelectedRow>
{
	public string? Label; // ToString() value, can be null
	public int RowIndex = -1; // Index in original list without filtering, todo: next schema change to nullable

	[NonSerialized]
	public object? Object; // used for bookmark searches, dangerous to keep these references around otherwise

	public string? DataKey;
	public object? DataValue; // Imported with bookmark into it's App DataRepo

	// public bool Pinned;
	// public List<string> SelectedColumns = new(); // Not supported yet

	public override string? ToString() => DataKey ?? Label;

	public SelectedRow() { }

	public SelectedRow(object obj)
	{
		Object = obj;
		if (obj == null) return; // obj can still be null

		Label = obj.ToString();

		DataKey = ObjectUtils.GetDataKey(obj); // overrides label
		DataValue = ObjectUtils.GetDataValue(obj);

		// Use the DataValue's DataKey if no DataKey found
		if (DataKey == null && DataValue != null)
		{
			DataKey = ObjectUtils.GetDataKey(DataValue);
		}

		Type type = obj.GetType();
		if (Label == type.FullName)
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
			   (RowIndex == other.RowIndex || RowIndex < 0 || other.RowIndex < 0); // todo: change RowIndex to nullable
	}

	public override int GetHashCode()
	{
		return (Label?.GetHashCode() ?? 0)
			^ (DataKey?.GetHashCode() ?? 0)
			^ (DataValue?.GetHashCode() ?? 0)
			^ RowIndex.GetHashCode();
	}
}
