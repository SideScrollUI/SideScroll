using Avalonia.Controls;
using SideScroll.UI.Avalonia.Samples.Tabs;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.UI.Avalonia.Charts.LiveCharts;

namespace SideScroll.UI.Avalonia.Samples;

public class MainWindow : BaseWindow
{
	public MainWindow() : base(Project.Load<CustomUserSettings>(Settings))
	{
		AddTab(new TabAvalonia());

		LiveChartCreator.Register();
		TabViewer.Toolbar?.AddVersion();

		Icon = new WindowIcon(Assets.Icons.SideScroll.Stream);
	}

	public static ProjectSettings Settings => new()
	{
		Name = "SideScroll",
		LinkType = "sidescroll",
		Version = ProjectSettings.ProgramVersion(),
		DataVersion = new Version(0, 1),
	};
}
