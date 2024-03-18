using Atlas.Core;
using Atlas.Resources;
using Atlas.Serialize;
using Atlas.UI.Avalonia;
using Atlas.UI.Avalonia.Themes.Tabs;

namespace Atlas.Tabs.Tools;

public class TabAvaloniaSettings : ITab
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
		public UserSettings? UserSettings;

		public override void Load(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonReset.Action = Reset;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);

			UserSettings = Project.UserSettings.DeepClone()!;
			model.AddObject(UserSettings);

			model.Items = new List<ListItem>()
			{
				new("Themes", new TabAvaloniaThemes()),
			};
		}

		private void Reset(Call call)
		{
			Project.UserSettings = new()
			{
				ProjectPath = UserSettings!.ProjectPath,
			};
			Reload();
		}

		private void Save(Call call)
		{
			DataApp.Save(UserSettings!);
			Project.UserSettings = UserSettings.DeepClone()!;

			ThemeManager.Current?.LoadCurrentTheme();
		}
	}
}
