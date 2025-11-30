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
		public ToolButton ButtonNew { get; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; } = new("Save", Icons.Svg.Save, isDefault: true);
	}

	public class Instance : TabInstance
	{
		private const string GroupId = "SampleParams";
		private const string DataKey = "Default";

		private SampleItem? _sampleItem;
		private TabFormObject? _tabFormObject;

		private DataRepoView<SampleItem>? _dataRepoView;

		public override void Load(Call call, TabModel model)
		{
			LoadSavedItems(call, model);

			_sampleItem ??= LoadData<SampleItem>(DataKey) ?? SampleItem.CreateSample();
			_tabFormObject = model.AddForm(_sampleItem);

			Toolbar toolbar = new();
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoView = Data.App.LoadView<SampleItem>(call, GroupId, nameof(SampleItem.Name));
			DataRepoInstance = _dataRepoView; // Allow links to pass the selected items

			if (_dataRepoView.Items.Count == 0)
			{
				for (int i = 0; i < 10; i++)
				{
					SampleItem sampleItem = new()
					{
						Name = "Item " + i,
						Amount = i * 10,
						Boolean = i % 2 == 0,
						DateTime = DateTime.Now.AddHours(i),
						Description = "Describe all the things",
					};
					_dataRepoView.Save(call, sampleItem);
				}
			}

			var dataCollection = new DataViewCollection<SampleItem, TabSampleItem>(_dataRepoView);
			model.Items = dataCollection.Items;
		}

		private void New(Call call)
		{
			_sampleItem = new();
			_tabFormObject!.Update(this, _sampleItem);
		}

		private void Save(Call call)
		{
			Validate();

			SampleItem clone = _sampleItem!.DeepClone(call);
			_dataRepoView!.Save(call, clone);
			SaveData(DataKey, clone);
		}
	}
}
