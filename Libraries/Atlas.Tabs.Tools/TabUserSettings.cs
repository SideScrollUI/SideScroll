using Atlas.Core;
using Atlas.Resources;
using Atlas.Serialize;
using Atlas.Tabs.Toolbar;

namespace Atlas.Tabs.Tools;

public class TabUserSettings : ITab
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
		}

		private void Reset(Call call)
		{
			Project.UserSettings = new();
			Refresh();
		}

		private void Save(Call call)
		{
			DataApp.Save(UserSettings!);
			Project.UserSettings = UserSettings.DeepClone()!;
		}
	}
}
