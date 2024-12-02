using SideScroll.Tabs;
using SideScroll.UI.Avalonia.Charts.LiveCharts;
using SideScroll.UI.Avalonia.Samples.Tabs;

namespace SideScroll.UI.Avalonia.Samples;

public class MainView : BaseView
{
	public MainView() : base(new Project(SampleProjectSettings.Settings))
	{
		AddTab(new TabAvalonia());

		LiveChartCreator.Register();
	}
}
