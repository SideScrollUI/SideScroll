using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Time;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Tabs.Samples.Forms;

[PublicData]
public class SampleItem
{
	[DataKey, Required, StringLength(30)]
	public string? Name { get; set; }

	[WordWrap, Watermark("Description"), AcceptsReturn, MinWidth(350)]
	public string? Description { get; set; }

	[ReadOnly(true)]
	public string? ReadOnly { get; set; }

	public bool Boolean { get; set; } = true;

	[Range(1, 1000), Required]
	public int Amount { get; set; } = 1;

	[ColumnIndex(2)]
	public double Double { get; set; }

	public AttributeTargets EnumAttributeTargets { get; set; } = AttributeTargets.Event;

	public static List<SampleListItem> ListItems { get; } =
	[
		new("One", 1),
		new("Two", 2),
		new("Three", 3),
	];

	[BindList(nameof(ListItems)), ColumnIndex(2)]
	public SampleListItem ListItem { get; set; } = ListItems[1];

	public DateTime DateTime { get; set; } = TimeZoneView.Now.Trim();

	public static List<TimeZoneView> TimeZones => TimeZoneView.All;

	[BindList(nameof(TimeZones))]
	public TimeZoneView TimeZone { get; set; } = TimeZoneView.Current;

	public override string? ToString() => Name;

	public static SampleItem CreateSample() => new()
	{
		Name = "Test",
		Description = "All the descriptions",
		ReadOnly = "ReadOnly",
	};
}

[PublicData]
public class SampleListItem(string name, int value)
{
	public string? Name { get; set; } = name;
	public int Value { get; set; } = value;

	public override string? ToString() => Name;
}
