using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Tabs.Settings;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Tools;

// See Avalonia version: TabAvaloniaSettings
public class TabUserSettings : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonReset { get; } = new("Reset", Icons.Svg.Reset);

		[Separator]
		public ToolButton ButtonSave { get; } = new("Save", Icons.Svg.Save);
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

			UserSettings = Project.UserSettings.DeepClone(call);
			model.AddForm(UserSettings);
		}

		private void Reset(Call call)
		{
			Project.UserSettings = Project.ProjectSettings.DefaultUserSettings;
			Refresh();
		}

		private void Save(Call call)
		{
			Data.App.Save(UserSettings!, call);
			Project.UserSettings = UserSettings!.DeepClone(call);
		}
	}
}
