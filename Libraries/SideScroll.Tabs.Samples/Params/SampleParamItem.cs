using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Time;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Tabs.Samples.Params;

[Params, PublicData]
public class SampleParamItem
{
	[DataKey, Required, StringLength(30)]
	public string Name { get; set; } = "Test";

	[WordWrap, Watermark("Description")]
	public string? Description { get; set; }

	[ReadOnly(true)]
	public string ReadOnly { get; set; } = "ReadOnly";

	public bool Boolean { get; set; } = true;

	[Range(1, 1000), Required]
	public int Amount { get; set; } = 123;

	[ColumnIndex(2)]
	public double Double { get; set; } = 3.14;

	public AttributeTargets EnumAttributeTargets { get; set; } = AttributeTargets.Event;

	public static List<ParamListItem> ListItems { get; } =
	[
		new("One", 1),
		new("Two", 2),
		new("Three", 3),
	];

	[BindList(nameof(ListItems)), ColumnIndex(2)]
	public ParamListItem ListItem { get; set; }

	public DateTime DateTime { get; set; } = TimeZoneView.Now.Trim();

	public static List<TimeZoneView> TimeZones => TimeZoneView.All;

	[BindList(nameof(TimeZones))]
	public TimeZoneView TimeZone { get; set; } = TimeZoneView.Current;

	public SampleParamItem()
	{
		ListItem = ListItems[1];
	}

	public override string ToString() => Name;
}

[PublicData]
public class ParamListItem(string name, int value)
{
	public string? Name { get; set; } = name;
	public int Value { get; set; } = value;

	public override string? ToString() => Name;
}
