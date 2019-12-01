using Atlas.Tabs;
using Atlas.GUI.Avalonia;
using Atlas.Start.Avalonia.Tabs;

namespace Atlas.Start.Avalonia
{
	public class MainWindow : BaseWindow
	{
		public MainWindow() : base(LoadProject())
		{
			AddClipBoardButtons();

			AddTab(new TabAvalonia());
		}

		public static Project LoadProject()
		{
			var projectSettings = new ProjectSettings()
			{
				Name = "Atlas",
				Version = "1",
				DataVersion = "1",
				LinkType = "atlas",
			};
			var userSettings = new UserSettings()
			{
				ProjectPath = UserSettings.DefaultProjectPath,
			};
			Project project = new Project(projectSettings, userSettings);
			return project;
		}
	}
}
