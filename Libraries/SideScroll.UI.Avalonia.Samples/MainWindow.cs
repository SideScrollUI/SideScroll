using Avalonia.Controls;
using SideScroll.Tabs;
using SideScroll.UI.Avalonia.Charts.LiveCharts;
using SideScroll.UI.Avalonia.Samples.Tabs;

namespace SideScroll.UI.Avalonia.Samples;

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
