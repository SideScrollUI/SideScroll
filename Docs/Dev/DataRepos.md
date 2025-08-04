# DataRepos

* Use DataRepos to store records locally using the [Serializer](Serializer.md)
* Each DataRepo is stored in a folder based on the `UserSettings.ProjectPath`, `<Type>`, and optional `GroupId`
* Use `DataRepo.LoadView<Type>()` to load a databound list that can be updated
* The `DataRepoView` can also be loaded into a `DataViewCollection<TDataType, TViewType>` to display a list of `TViewType` that implement `IDataView`, and optionally an `ITab` to show a tab as well
* To enable linking DataRepo items, set the `TabInstance.DataRepoInstance` to a `DataRepoView`

## Default Storage Locations

- Windows
  - Save Data: `C:\Users\<Username>\AppData\Roaming\[<Domain>\]<ProjectName>`
  - Local & Cache: `C:\Users\<Username>\AppData\Local\[<Domain>\]<ProjectName>`
- MacOS
  - All Data: `/Users/<user>/Library/Application Support/[<Domain>/]<ProjectName>`
- Linux
  - Save Data: `/home/<user>/.config/[<Domain>/]<ProjectName>`
  - Local & Cache: `/home/<user>/.local/share/[<Domain>/]<ProjectName>`

## Sample Param DataRepo Tab

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
			DataRepoInstance = _dataRepoView; // Allow links to pass the selected items

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
[Source](../../Libraries/SideScroll.Tabs.Samples/Forms/TabSampleFormDataTabs.cs)

## Attributes

- `[DataKey]` - The first member that specifies this will be used as the object's Id
- `[DataValue]` - This member will be imported with links into the tab's `DataRepoInstance` if both are set

## DataRepo Index

- For large data repos with more than a hundred records, adding a `DataRepoIndex` can be used to load a page at a time. This can be enabled by opening a `DataRepo` with `indexed = true`, or calling `DataRepoInstance.AddIndex()`
- Objects are currently sorted in the order they were added. More complex index types will eventually be supported
- [Sample](../../Libraries/SideScroll.Tabs.Samples/DataRepo/TabSampleDataRepoPaging.cs)
