using Atlas.Resources;
using Atlas.Start.Avalonia.Tabs;
using Atlas.Tabs;
using Atlas.UI.Avalonia;
using Atlas.UI.Avalonia.Charts.LiveCharts;
using Atlas.UI.Avalonia.ScreenCapture;
using Avalonia.Controls;

namespace Atlas.Start.Avalonia;

public class MainWindow : BaseWindow
{
	public MainWindow() : base(new Project(Settings))
	{
		AddTab(new TabAvalonia());

		LiveChartCreator.Register();
		ScreenCapture.AddControlTo(TabViewer);
		TabViewer.Toolbar?.AddVersion();

		Icon = new WindowIcon(Icons.Logo.Stream);
	}

	public static ProjectSettings Settings => new()
	{
		Name = "Atlas",
		LinkType = "atlas",
		Version = ProjectSettings.ProgramVersion(),
		DataVersion = new Version(1, 1),
	};
}
