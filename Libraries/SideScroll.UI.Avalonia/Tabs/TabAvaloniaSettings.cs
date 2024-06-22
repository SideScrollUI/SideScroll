using SideScroll;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;
using SideScroll.UI.Avalonia.Themes;
using SideScroll.UI.Avalonia.Themes.Tabs;
using Avalonia;
using Avalonia.Styling;

namespace SideScroll.UI.Avalonia.Tabs;

public class TabAvaloniaSettings : TabAvaloniaSettings<UserSettings>;

public class TabAvaloniaSettings<T> : ITab where T : UserSettings, new()
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonReset { get; set; } = new("Reset", Icons.Svg.Reset);

		[Separator]
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save);
	}

	public class Instance : TabInstance
	{
		public T? CustomUserSettings;

		public override void LoadUI(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonReset.Action = Reset;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);

			LoadCustomSettings();
			model.AddObject(CustomUserSettings!);

			model.Items = new List<ListItem>
			{
				new("Themes", new TabAvaloniaThemes()),
			};
		}

		private void LoadCustomSettings()
		{
			if (Project.UserSettings.DeepClone() is T customUserSettings)
			{
				CustomUserSettings = customUserSettings;
			}
			else
			{
				CustomUserSettings = new();
			}

			if (CustomUserSettings.Theme == null && UserSettings.Themes.Count > 0)
			{
				if (Application.Current?.ActualThemeVariant is ThemeVariant variant &&
					variant.Key is string key &&
					UserSettings.Themes.Contains(key))
				{
					CustomUserSettings.Theme = key;
				}
			}
		}

		private void Reset(Call call)
		{
			Project.UserSettings = new()
			{
				ProjectPath = CustomUserSettings!.ProjectPath,
			};
			Reload();
		}

		private void Save(Call call)
		{
			DataApp.Save(CustomUserSettings!);
			Project.UserSettings = CustomUserSettings.DeepClone()!;

			ThemeManager.Current?.LoadCurrentTheme();
		}
	}
}
