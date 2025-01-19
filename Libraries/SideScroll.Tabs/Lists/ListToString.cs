using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Utilities;
using System.Collections;

namespace SideScroll.Tabs.Lists;

[Skippable]
public class ListToString
{
	public static int MaxItems { get; set; } = 200_000;

	[InnerValue, HiddenColumn]
	public object Object { get; set; }

	public string? Value { get; set; }

	[DataKey, HiddenColumn]
	public string? DataKey { get; set; }

	[DataValue, HiddenColumn]
	public object? DataValue { get; set; }

	public override string? ToString() => Value;

	public ListToString(object obj)
	{
		Object = obj;
		if (obj == null)
			return;

		Value = obj.ToString();

		DataKey = ObjectUtils.GetDataKey(obj);
		DataValue = ObjectUtils.GetDataValue(obj);
	}

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
