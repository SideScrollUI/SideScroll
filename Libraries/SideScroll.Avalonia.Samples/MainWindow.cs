using Avalonia.Controls;
using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Tabs;

namespace SideScroll.Avalonia.Samples;

public class MainWindow : BaseWindow
{
	public MainWindow() : base(Project.Load<CustomUserSettings>(SampleProjectSettings.Settings))
	{
		AddTab(new TabAvalonia());

		LiveChartCreator.Register();
		TabViewer.Toolbar?.AddVersion();

		Icon = new WindowIcon(Assets.Icons.SideScroll.Stream);
	}
}
