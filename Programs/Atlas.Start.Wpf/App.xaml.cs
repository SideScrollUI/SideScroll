using Atlas.Tabs;
using System;
using System.Windows;

namespace Atlas.Start.Wpf
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			/*if (projectPath == null || projectPath.Length == 0)
			{
				Content = new NewProject(this);
				return;
			}*/

			Project project = new Project(Settings);
			MainWindow window = new MainWindow(project);
			//window.LoadSettings(Settings.DefaultProjectPath);
			window.Show();
		}

		public static ProjectSettings Settings => new ProjectSettings()
		{
			Name = "Atlas",
			LinkType = "atlas",
			Version = new Version(1, 0),
			DataVersion = new Version(1, 0),
		};

		public void ShowNewProject()
		{
			NewProject newProject = new NewProject(this);
		}
	}
}
