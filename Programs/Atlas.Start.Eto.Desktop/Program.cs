using System;
using Eto;
using Eto.Forms;
using Atlas.Start.Eto;
using Atlas.GUI.Eto;
using Atlas.Tabs;

namespace Atlas.Start.Eto.Desktop
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var pf = Platform.Detect;
			/*if (pf.IsWpf)
			{
				pf.Add(typeof(TabView), () => new Eto.Forms.Control.Wpf.PlotHandler());
			}*/
			new Application(pf).Run(new MainWindow(ProjectSettings.DefaultProjectPath));
		}
	}
}
