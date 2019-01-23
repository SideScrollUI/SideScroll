using Atlas.Tabs;
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

			//string projectPath = WpfWindow.LoadProject(Settings.DefaultProjectPath);
			MainWindow window = new MainWindow(ProjectSettings.DefaultProjectPath);
			//window.LoadSettings(Settings.DefaultProjectPath);
			window.Show();
		}

		public void ShowNewProject()
		{
			NewProject newProject = new NewProject(this);
		}
	}
}
