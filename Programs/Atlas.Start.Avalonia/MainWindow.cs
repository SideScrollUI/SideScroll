using Atlas.Tabs;
using Atlas.GUI.Avalonia;
using Atlas.Start.Avalonia.Tabs;

namespace Atlas.Start.Avalonia
{
	public class MainWindow : BaseWindow
	{
		public static Project defaultProject; // todo: find a way to pass this in

		public MainWindow() : base(defaultProject)
		{
			AddTabView(new TabAvalonia.Instance(defaultProject));
		}
	}
}
