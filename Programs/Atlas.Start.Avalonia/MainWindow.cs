using Atlas.Start.Avalonia.Tabs;
using Atlas.Tabs;
using Atlas.UI.Avalonia;
using Atlas.UI.Avalonia.Charts.LiveCharts;
using Atlas.UI.Avalonia.Charts.OxyPlots;
using Atlas.UI.Avalonia.ScreenCapture;

namespace Atlas.Start.Avalonia;

public class MainWindow : BaseWindow
{
	public MainWindow() : base(new Project(Settings))
	{
		AddTab(new TabAvalonia());

		LiveChartCreator.Register();
		//OxyPlotCreator.Register();
		ScreenCapture.AddControlTo(TabViewer);
	}

	public static ProjectSettings Settings => new()
	{
		Name = "Atlas",
		LinkType = "atlas",
		Version = ProjectSettings.ProgramVersion(),
		DataVersion = new Version(1, 1),
	};
}
