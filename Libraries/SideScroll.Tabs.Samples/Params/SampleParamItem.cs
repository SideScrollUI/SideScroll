using SideScroll.Attributes;
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

	public static List<ParamListItem> ListItems =>
	[
		new("One", 1),
		new("Two", 2),
		new("Three", 3),
	];

	[BindList(nameof(ListItems)), ColumnIndex(2)]
	public ParamListItem ListItem { get; set; }

	public DateTime DateTime { get; set; } = DateTime.Now;

	public static List<TimeZoneView> TimeZones => TimeZoneView.All;

	[BindList(nameof(TimeZones))]
	public TimeZoneView TimeZone { get; set; } = TimeZoneView.Utc;

	public SampleParamItem()
	{
		ListItem = ListItems[1];
	}

	public override string ToString() => Name;
}

[PublicData]
public class ParamListItem
{
	public string? Name { get; set; }
	public int Value { get; set; }

	public override string? ToString() => Name;

	public ParamListItem() { }

	public ParamListItem(string name, int value)
	{
		Name = name;
		Value = value;
	}
}
