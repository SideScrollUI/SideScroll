using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Samples.Models;
using SideScroll.Tabs.Toolbar;
using System.Text.Json;

namespace SideScroll.Tabs.Samples.Demo;

public class TabDemoPlanets : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonReset { get; set; } = new("Reset", Icons.Svg.Reset);

		[Separator]
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonNew { get; set; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save);
		public ToolButton ButtonDelete { get; set; } = new("Delete", Icons.Svg.Delete);

		[Separator]
		public ToolButton ButtonCopyToClipboard { get; set; } = new("Copy to Clipboard", Icons.Svg.Copy);
	}

	public class Instance : TabInstance
	{
		private const string GroupId = "Planets";

		private DataRepoView<Planet>? _dataRepoView;
		private Planet? _planet;

		public override void LoadUI(Call call, TabModel model)
		{
			_planet = Planet.CreateSample();
			model.AddForm(_planet);

			Toolbar toolbar = new();
			toolbar.ButtonReset.Action = Reset;
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			toolbar.ButtonDelete.Action = Delete;
			toolbar.ButtonCopyToClipboard.Action = CopyClipBoardUI;
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
			_planet!.Clear();
			Refresh();
		}

		private void Save(Call call)
		{
			Validate();

			var clone = _planet.DeepClone()!;

			_dataRepoView!.Save(call, clone);
		}

		private void Delete(Call call)
		{
			foreach (Planet planet in SelectedItems!)
			{
				_dataRepoView!.Delete(call, planet);
			}
		}

		private void CopyClipBoardUI(Call call)
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			string json = JsonSerializer.Serialize(SelectedItems, options);
			CopyToClipboard(json);
		}
	}
}
