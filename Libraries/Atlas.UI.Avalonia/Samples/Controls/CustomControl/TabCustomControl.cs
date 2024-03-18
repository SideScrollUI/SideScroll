using Atlas.Core;
using Atlas.Tabs;
using Atlas.Tabs.Test.Models;
using Atlas.UI.Avalonia.Controls;
using System.Text.Json;

namespace Atlas.UI.Avalonia.Samples.Controls.CustomControl;

public class TabCustomControl : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private ItemCollectionUI<Planet> _planets = new();
		private Planet? _planet;
		private TabControlSearchToolbar? _toolbar;

		public override void LoadUI(Call call, TabModel model)
		{
			_planet = new Planet()
			{
				Name = "Planet X",
				DistanceKm = 10_000_000_000,
				RadiusKm = 5_000,
				MassKg = 2,
				OrbitalPeriodDays = 60_000,
				Inner = false,
			};

			var planetParams = new TabControlParams(_planet);
			model.AddObject(planetParams);

			_toolbar = new TabControlSearchToolbar(this);
			model.AddObject(_toolbar);

			_toolbar.ButtonSearch.Add(SearchUI);
			_toolbar.ButtonCopyClipBoard.Add(CopyClipBoardUI);

			_planets = new ItemCollectionUI<Planet>(SolarSystem.Sample.Planets);
			model.Items = _planets;
		}

		private void SearchUI(Call call)
		{
			_toolbar!.TextBoxStatus.Text = "Searching";

			StartAsync(SearchAsync, call, true);
		}

		private async Task SearchAsync(Call call)
		{
			await Task.Delay(2000);

			Invoke(ShowSearchResults, 1, "abc");
		}

		private void ShowSearchResults(Call call, params object[] objects)
		{
			_toolbar!.TextBoxStatus.Text = "Finished";
		}

		private void CopyClipBoardUI(Call call)
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			string json = JsonSerializer.Serialize(_planets, options);
			ClipboardUtils.SetText(_toolbar, json);
		}
	}
}
