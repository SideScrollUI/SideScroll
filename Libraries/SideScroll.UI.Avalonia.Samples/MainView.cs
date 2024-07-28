using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.UI.Avalonia.Samples.Tabs;
//using SideScroll.UI.Avalonia.Charts.LiveCharts;
//using SideScroll.UI.Avalonia.ScreenCapture;

namespace SideScroll.UI.Avalonia.Samples;

public class MainView : BaseView
{
	public MainView() : base(new Project(Settings))
	{
		AddTab(new TabAvalonia());

		//LiveChartCreator.Register();
		//ScreenCapture.AddControlTo(TabViewer);
	}

	public static ProjectSettings Settings => new()
	{
		Name = "SideScroll",
		LinkType = "sidescroll",
		Version = ProjectSettings.ProgramVersion(),
		DataVersion = new Version(1, 1),
	};
}
