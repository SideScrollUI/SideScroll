using Atlas.Tabs;
using Atlas.GUI.Avalonia;
using Atlas.Start.Avalonia.Tabs;
using System;

namespace Atlas.Start.Avalonia
{
	public class MainWindow : BaseWindow
	{
		public MainWindow() : base(LoadProject())
		{
			AddTab(new TabAvalonia());
		}

		public static Project LoadProject()
		{
			var projectSettings = new ProjectSettings()
			{
				Name = "Atlas",
				Version = new Version(1, 0),
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
