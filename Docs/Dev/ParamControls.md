# Param Controls

* Param controls allow editing objects by automatically mapping visible properties to a matching UI control
* Adding a class of type `[Params]` to a `TabModel` creates a `TabControlParams`

#### Sample Param Tab
```csharp
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Params;

public class TabSampleParamsDataTabs : ITab
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
		private const string DataKey = "Params";

		private SampleParamItem? _sampleParamItem;
		private DataRepoView<SampleParamItem>? _dataRepoView;

		public override void Load(Call call, TabModel model)
		{
			LoadSavedItems(call, model);

			_sampleParamItem = LoadData<SampleParamItem>(DataKey);
			model.AddObject(_sampleParamItem!);

			Toolbar toolbar = new();
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoView = DataApp.LoadView<SampleParamItem>(call, "SampleParams", nameof(SampleParamItem.Name));
			DataRepoInstance = _dataRepoView;

			var dataCollection = new DataViewCollection<SampleParamItem, TabSampleParamItem>(_dataRepoView);
			model.Items = dataCollection.Items;
		}

		private void New(Call call)
		{
			Reload();
		}

		private void Save(Call call)
		{
			Validate();

			SampleParamItem clone = _sampleParamItem.DeepClone(call)!;
			_dataRepoView!.Save(call, clone.ToString(), clone);
			SaveData(DataKey, clone);
		}
	}
}
```
[Source](../../Libraries/SideScroll.Tabs.Samples/Params/TabSampleParamsDataTabs.cs)

#### Sample Param Item
```csharp
using SideScroll.Attributes;
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
```
[Source](../../Libraries/SideScroll.Tabs.Samples/Params/SampleParamItem.cs)

## Custom Controls
- Pass any `object` to a `TabControlParams` to allow updating that control directly
```csharp
var planetParams = new TabControlParams(_planet);
model.AddObject(planetParams);
```
[Source](../../Libraries/SideScroll.UI.Avalonia/Samples/Controls/CustomControl/TabCustomControl.cs)

## Attributes
- `[AcceptsReturn]` - Allow return key to add a new line
- `[Watermark(string text)]` - Show the specified watermark text when no value is specified
- `[PasswordChar(char c)]` - Sets the password character to show instead of the actual text
- `[BindList(string propertyName)]` - The member name that contains a list of items to select this item's value from
- `[Header(string text)]` - Show a title and separator before this item
- `[Separator]` - Show a separator before this item
- `[ColumnIndex(int index)]` - Sets the column to use. This should usually be in multiples of 2 since a label & control use two columns

### Links
- [Sample Tabs](../../Libraries/SideScroll.Tabs.Samples/Params/TabSampleParams.cs)