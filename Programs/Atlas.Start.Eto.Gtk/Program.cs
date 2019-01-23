using System;
using Atlas.Tabs;
using Eto.Forms;

namespace Atlas.Start.Eto.Gtk
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application(global::Eto.Platforms.Gtk).Run(new MainWindow(ProjectSettings.DefaultProjectPath));
		}
	}
}

/*
How to install GTK?
Same as Desktop, but allows a dotcore app to be made
*/