using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Tabs;

namespace SideScroll.Avalonia.Samples;

public class MainView : BaseView
{
	public MainView() : base(Project.Load(SampleProjectSettings.Settings))
	{
		AddTab(new TabAvalonia());

		LiveChartCreator.Register();
		TabViewer.Toolbar?.AddVersion();
	}
}
