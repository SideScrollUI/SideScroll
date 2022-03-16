using Atlas.Core;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs;

[PublicData]
public class SelectedRow
{
	public string Label; // null if ToString() returns type
	public int RowIndex;

	[NonSerialized]
	public object Object; // used for bookmark searches, dangerous to keep these references around otherwise

	public string DataKey;
	public object DataValue;

	//public bool Pinned;
	public List<string> SelectedColumns = new();

	public override string ToString() => Label;

	public SelectedRow() { }

	public SelectedRow(object obj)
	{
		Object = obj;
		Label = obj.ToString();

		DataKey = ObjectUtils.GetDataKey(obj); // overrides label
		DataValue = ObjectUtils.GetDataValue(obj);

		// Use the DataValue's DataKey if no DataKey found
		if (DataKey == null && DataValue != null)
			DataKey = ObjectUtils.GetDataKey(DataValue);

		Type type = obj.GetType();
		if (Label == type.FullName)
			Label = null;
	}
}
