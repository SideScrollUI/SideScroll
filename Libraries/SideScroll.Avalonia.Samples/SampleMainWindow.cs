using Avalonia.Controls;
using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Tabs;

namespace SideScroll.Avalonia.Samples;

public class SampleMainWindow : BaseWindow
{
	public SampleMainWindow() : base(Project.Load<SampleUserSettings>(SampleProjectSettings.Default))
	{
		Icon = new WindowIcon(Assets.Icons.SideScroll.Stream);

		LiveChartCreator.Register();
		TabViewer.Toolbar?.AddRightControls();

		LoadTab(new TabAvaloniaSamples());
	}
}
