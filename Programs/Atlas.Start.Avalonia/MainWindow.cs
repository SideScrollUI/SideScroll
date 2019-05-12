using Atlas.Tabs;
using Atlas.GUI.Avalonia;
using Atlas.Start.Avalonia.Tabs;

namespace Atlas.Start.Avalonia
{
	public class MainWindow : BaseWindow
	{
		public MainWindow() : base()
		{
			LoadProject(ProjectSettings.DefaultProjectPath);
		}

		public void LoadProject(string projectPath)
		{
			Project project = new Project(projectPath, typeof(MainWindow).Namespace);
			LoadProject(project);

			AddTabView(new TabAvalonia.Instance(project));
			//AddTab(new TabAvalonia());
		}
	}
}
