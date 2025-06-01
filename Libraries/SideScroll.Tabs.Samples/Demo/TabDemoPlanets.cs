using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Tabs.Samples.Models;
using SideScroll.Tabs.Toolbar;
using System.Text.Json;

namespace SideScroll.Tabs.Samples.Demo;

public class TabDemoPlanets : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonNew { get; set; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save);

		[Separator]
		public ToolButton ButtonCopyToClipboard { get; set; } = new("Copy to Clipboard", Icons.Svg.Copy);
	}

	public class Instance : TabInstance
	{
		private ItemCollectionUI<Planet>? _planets;
		private Planet? _planet;

		public override void LoadUI(Call call, TabModel model)
		{
			_planet = Planet.CreateSample();
			model.AddObject(_planet);

			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonNew.Action = Clear;
			toolbar.ButtonSave.Action = Save;
			toolbar.ButtonCopyToClipboard.Action = CopyClipBoardUI;
			model.AddObject(toolbar);

			_planets ??= [.. SolarSystem.Sample.Planets];
			model.Items = _planets;
		}

		private void Refresh(Call call)
		{
			Reload();
		}

		private void Clear(Call call)
		{
			_planet!.Clear();
			Refresh();
		}

		private void Save(Call call)
		{
			Validate();
			_planets!.Add(_planet.DeepClone()!);
		}

		private void CopyClipBoardUI(Call call)
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			string json = JsonSerializer.Serialize(_planets, options);
			CopyToClipboard(json);
		}
	}
}
