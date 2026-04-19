using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Logs;

namespace SideScroll.Demo.Avalonia.Browser;

public class BrowserMainView : BaseView
{
	/// <summary>
	/// Global log for browser application - logs appear in browser console via LogWriterConsole
	/// </summary>
	public static Log AppLog { get; } = new();

	private LogWriterConsole? _logWriter;

	public BrowserMainView() : base(BrowserProject.Load())
	{
		// Hook up console logging for all AppLog messages
		_logWriter = new LogWriterConsole(AppLog);

		LoadTab(new TabAvaloniaSamples());
		LiveChartCreator.Register();
		TabViewer.Toolbar?.AddRightControls();
	}
}
