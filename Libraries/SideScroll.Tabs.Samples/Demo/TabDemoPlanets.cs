using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Samples.Models;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Demo;

public class TabDemoPlanets : ITab
{
	public TabInstance Create() => new Instance();

	private class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonNew { get; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; } = new("Save", Icons.Svg.Save, isDefault: true);

		[Separator]
		public ToolButton ButtonDelete { get; } = new("Delete", Icons.Svg.Delete);

		[Separator]
		public ToolButton ButtonCopyToClipboard { get; } = new("Copy to Clipboard", Icons.Svg.Copy);

		[Separator]
		public ToolButton ButtonReset { get; } = new("Reset", Icons.Svg.Reset);
	}

	private class Instance : TabInstance
	{
		private const string GroupId = "Planets";

		private DataRepoView<Planet>? _dataRepoView;
		private Planet? _planet;
		private TabFormObject? _tabFormObject;

		public override void Load(Call call, TabModel model)
		{
			_planet = Planet.CreateSample();
			_tabFormObject = model.AddForm(_planet);

			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			toolbar.ButtonDelete.Action = Delete;
			toolbar.ButtonCopyToClipboard.Action = CopyClipBoard;
			toolbar.ButtonReset.Action = Reset;
			model.AddObject(toolbar);

			LoadSavedItems(call, model);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoView = Data.App.LoadIndexedView<Planet>(call, GroupId);
			DataRepoInstance = _dataRepoView; // Allow links to pass the selected items

			if (_dataRepoView.Items.Count == 0)
			{
				_dataRepoView.Save(call, SolarSystem.Sample.Planets);
			}

			var dataCollection = new DataViewCollection<Planet>(_dataRepoView);
			model.Items = dataCollection.Items;
		}

		private void Reset(Call call)
		{
			_dataRepoView!.DeleteAll(call);
			Reload();
		}

		private void Refresh(Call call)
		{
			Reload();
		}

		private void New(Call call)
		{
			_planet = new();
			_tabFormObject!.Update(this, _planet);
		}

		private void Save(Call call)
		{
			Validate();

			var clone = _planet!.DeepClone();

			_dataRepoView!.Save(call, clone);

			New(call);
		}

		private void Delete(Call call)
		{
			List<Planet> selected = SelectedItems!
				.OfType<Planet>()
				.ToList();

			foreach (Planet planet in selected)
			{
				_dataRepoView!.Delete(call, planet);
			}
		}

		private void CopyClipBoard(Call call)
		{
			CopyToClipboard(SelectedItems);
		}
	}
}
