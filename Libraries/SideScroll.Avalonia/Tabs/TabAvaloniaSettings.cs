using Avalonia.Controls;
using SideScroll.Attributes;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Themes.Tabs;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Settings;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using SideScroll.Time;

namespace SideScroll.Avalonia.Tabs;

public class TabAvaloniaSettings : TabAvaloniaSettings<UserSettings>;

public class TabAvaloniaSettings<T> : ITab where T : UserSettings, new()
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonReset { get; set; } = new("Reset", Icons.Svg.Reset)
		{
			Flyout = new ConfirmationFlyoutConfig("Are you sure you want to reset the settings?", "Reset"),
		};

		[Separator]
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save);
	}

	public class Instance : TabInstance
	{
		public T? CustomUserSettings;

		public override void LoadUI(Call call, TabModel model)
		{
			model.MaxDesiredWidth = 500;

			Toolbar toolbar = new();
			toolbar.ButtonReset.Action = Reset;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);

			LoadCustomSettings();
			model.AddForm(CustomUserSettings!);

			model.Items = new List<ListItem>
			{
				new("Themes", new TabAvaloniaThemes()),
				new("Data", new TabDataRepoSettings(CustomUserSettings!)),
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

			CustomUserSettings.Theme ??= SideScrollTheme.ThemeVariant.ToString();
		}

		private void Reset(Call call)
		{
			Project.UserSettings = Project.ProjectSettings.DefaultUserSettings;
			Reload();
		}

		private void Save(Call call)
		{
			Data.App.Save(CustomUserSettings!);
			Project.UserSettings = CustomUserSettings.DeepClone(call)!;

			TimeZoneView.Current = Project.UserSettings.TimeZone;
			DateTimeExtensions.DefaultFormatType = Project.UserSettings.TimeFormat;
			ThemeManager.Instance?.LoadCurrentTheme();
		}
	}
}
