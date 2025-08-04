# Tab Forms

* Tab Form controls allow editing objects by automatically mapping visible properties to a matching Avalonia control

### Sample Tab Form

```csharp
using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Forms;

[TabRoot, PublicData]
public class TabSampleFormDataTabs : ITab
{
	public override string ToString() => "Data Repos";

	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonNew { get; set; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save);
	}

	public class Instance : TabInstance
	{
		private const string GroupId = "SampleParams";
		private const string DataKey = "Params";

		private SampleItem? _sampleItem;
		private DataRepoView<SampleItem>? _dataRepoView;

		public override void Load(Call call, TabModel model)
		{
			LoadSavedItems(call, model);

			_sampleItem ??= LoadData<SampleItem>(DataKey);
			model.AddForm(_sampleItem!);

			Toolbar toolbar = new();
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoView = Data.App.LoadView<SampleItem>(call, GroupId, nameof(SampleItem.Name));
			DataRepoInstance = _dataRepoView; // Allow links to pass the selected items

			var dataCollection = new DataViewCollection<SampleItem, TabSampleItem>(_dataRepoView);
			model.Items = dataCollection.Items;
		}

		private void New(Call call)
		{
			_sampleItem = new();
			Reload();
		}

		private void Save(Call call)
		{
			Validate();

			SampleItem clone = _sampleItem.DeepClone(call)!;
			_dataRepoView!.Save(call, clone);
			SaveData(DataKey, clone);
		}
	}
}

```
[Source](../../Libraries/SideScroll.Tabs.Samples/Forms/TabSampleFormDataTabs.cs)

### Sample Param Item

```csharp
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

	public SampleItem()
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

```
[Source](../../Libraries/SideScroll.Tabs.Samples/Forms/SampleItem.cs)

## Custom Controls

- Pass any `object` to a `TabForm` to allow updating that control directly
```csharp
var planetForm = new TabForm(_planet);
model.AddObject(planetForm);
```
[Source](../../Libraries/SideScroll.Avalonia/Samples/Controls/CustomControl/TabCustomControl.cs)

## Attributes

| Attribute | Description |
| - | - |
| `[AcceptsReturn]` | Allow return key to add a new line |
| `[Watermark(string text)]` | Show the specified watermark text when no value is specified |
| `[PasswordChar(char c)]` | Sets the password character to show instead of the actual text |
| `[BindList(string propertyName)]` | The member name that contains a list of items to select this item's value from |
| `[Header(string text)]` | Show a title and separator before this item |
| `[Separator]` | Show a separator before this item |
| `[ColumnIndex(int index)]` | Sets the column to use. This should usually be in multiples of 2 since a label & control use two columns |

## Links

- [Sample Tabs](../../Libraries/SideScroll.Tabs.Samples/Forms/TabSampleForms.cs)