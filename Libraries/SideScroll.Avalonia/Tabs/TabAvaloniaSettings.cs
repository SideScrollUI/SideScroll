using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Themes.Tabs;
using SideScroll.Extensions;
using SideScroll.Serialize;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Settings;
using SideScroll.Tasks;
using SideScroll.Time;

namespace SideScroll.Avalonia.Tabs;

public class TabAvaloniaSettings : TabAvaloniaSettings<UserSettings>;

public class TabAvaloniaSettings<T> : ITab where T : UserSettings, new()
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		// Keep the concrete settings type so saving/loading use the same DataRepo key (typeof(T))
		private T? _customUserSettings;

		public override void LoadUI(Call call, TabModel model)
		{
			// A single cloned copy is shared across the child tabs so that saving from
			// either General or Data persists all pending edits
			_customUserSettings = LoadCustomSettings();

			model.Items = new List<ListItem>
			{
				new("General", new TabSettingsGeneral(_customUserSettings, Save, Reset)),
				new("Themes", new TabAvaloniaThemes()),
				new("Data", new TabDataRepoSettings(_customUserSettings, Save)),
			};
		}

		private T LoadCustomSettings()
		{
			T customUserSettings = Project.UserSettings.DeepClone() as T ?? new T();
			customUserSettings.Theme ??= SideScrollTheme.ThemeVariant.ToString();
			return customUserSettings;
		}

		private void Reset(Call call)
		{
			Project.UserSettings = Project.ProjectSettings.DefaultUserSettings;
			Reload();
		}

		private void Save(Call call)
		{
			Data.App.Save(_customUserSettings!);
			Project.UserSettings = _customUserSettings!.DeepClone(call);

			TimeZoneView.Current = Project.UserSettings.TimeZone;
			DateTimeExtensions.DefaultFormatType = Project.UserSettings.TimeFormat;
			ThemeManager.Instance?.LoadCurrentTheme();

			call.TaskInstance!.ShowMessage("Saved Settings");
		}
	}
}
