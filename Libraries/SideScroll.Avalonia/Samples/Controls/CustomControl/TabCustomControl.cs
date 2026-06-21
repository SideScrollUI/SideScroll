using SideScroll.Avalonia.Controls;
using SideScroll.Collections;
using SideScroll.Serialize;
using SideScroll.Tabs;
using SideScroll.Tabs.Samples.Models;

namespace SideScroll.Avalonia.Samples.Controls.CustomControl;

public class TabCustomControl : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		private ItemCollectionUI<Planet>? _planets;
		private Planet? _planet;

		private TabControlSearchToolbar? _toolbar;
		private TabForm? _planetForm;

		public override void LoadUI(Call call, TabModel model)
		{
			_planet = Planet.CreateSample();

			_planetForm = new TabForm(_planet);
			model.AddObject(_planetForm);

			_toolbar = new TabControlSearchToolbar(this);
			_toolbar.ButtonNew.Add(New);
			_toolbar.ButtonSave.Add(Save);
			_toolbar.ButtonSearch.Add(Search);
			_toolbar.ButtonCopyToClipboard.Add(CopyToClipboard);
			model.AddObject(_toolbar);

			_planets ??= [.. SolarSystem.Sample.Planets];
			model.AddItems(_planets);
		}

		private void New(Call call)
		{
			_planet = new();
			_planetForm!.LoadObject(_planet);
			_planetForm.Focus();
		}

		private void Save(Call call)
		{
			_planets!.Add(_planet!.DeepClone());
			New(call);
		}

		private void Search(Call call)
		{
			_toolbar!.TextBoxStatus.Text = "Searching";

			StartAsync(SearchAsync, call, true);
		}

		private async Task SearchAsync(Call call)
		{
			await Task.Delay(2000);

			Post(ShowSearchResults, 1, "abc");
		}

		private void ShowSearchResults(Call call, params object[] objects)
		{
			_toolbar!.TextBoxStatus.Text = "Finished";
		}

		private void CopyToClipboard(Call call)
		{
			CopyToClipboard(call, SelectedItems);
		}
	}
}
