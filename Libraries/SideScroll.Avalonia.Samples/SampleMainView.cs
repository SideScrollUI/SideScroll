using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Tabs;

namespace SideScroll.Avalonia.Samples;

public class SampleMainView : BaseView
{
	public SampleMainView() : base(Project.Load(SampleProjectSettings.Settings))
	{
		LoadTab(new TabAvaloniaSamples());

		LiveChartCreator.Register();
		TabViewer.Toolbar?.AddRightControls();
	}
}
