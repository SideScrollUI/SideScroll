using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Tabs;

namespace SideScroll.Avalonia.Samples;

public class MainView : BaseView
{
	public MainView() : base(new Project(SampleProjectSettings.Settings))
	{
		AddTab(new TabAvalonia());

		LiveChartCreator.Register();
	}
}
