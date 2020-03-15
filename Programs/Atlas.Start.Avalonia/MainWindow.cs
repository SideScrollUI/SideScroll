using System;
using Atlas.UI.Avalonia;
using Atlas.Start.Avalonia.Tabs;
using Atlas.Tabs;

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
				LinkType = "atlas",
				Version = new Version(1, 0),
				DataVersion = new Version(1, 0),
			};
			var userSettings = new UserSettings()
			{
				ProjectPath = UserSettings.DefaultProjectPath,
			};
			return new Project(projectSettings, userSettings);
		}
	}
}
