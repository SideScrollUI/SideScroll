using Atlas.Start.Avalonia.Tabs;
using Atlas.Tabs;
using Atlas.UI.Avalonia;
using Atlas.UI.Avalonia.ScreenCapture;
using System;

namespace Atlas.Start.Avalonia
{
	public class MainWindow : BaseWindow
	{
		public MainWindow() : base(new Project(Settings))
		{
			AddTab(new TabAvalonia());

			ScreenCapture.Initialize(TabViewer);
		}

		public static ProjectSettings Settings => new ProjectSettings()
		{
			Name = "Atlas",
			LinkType = "atlas",
			Version = ProjectSettings.ProgramVersion(),
			DataVersion = new Version(1, 1),
		};
	}
}
