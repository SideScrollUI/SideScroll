using Avalonia.Controls;
using SideScroll.Start.Avalonia.Tabs;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.UI.Avalonia;
using SideScroll.UI.Avalonia.Charts.LiveCharts;
using SideScroll.UI.Avalonia.ScreenCapture;

namespace SideScroll.Start.Avalonia;

public class MainWindow : BaseWindow
{
	public MainWindow() : base(Project.Load<CustomUserSettings>(Settings))
	{
		AddTab(new TabAvalonia());

		LiveChartCreator.Register();
		ScreenCapture.AddControlTo(TabViewer);
		TabViewer.Toolbar?.AddVersion();

		Icon = new WindowIcon(Assets.Icons.Logo.Stream);
	}

	public static ProjectSettings Settings => new()
	{
		Name = "SideScroll",
		LinkType = "sidescroll",
		Version = ProjectSettings.ProgramVersion(),
		DataVersion = new Version(0, 1),
	};
}
