using Atlas.Core;
using Atlas.UI.Wpf;
using Atlas.Tabs;
using System;
using System.Diagnostics;
using System.Threading;
using Atlas.Tabs.Test;

namespace Atlas.Start.Wpf
{
	public class MainWindow : BaseWindow
	{
		public MainWindow(Project project) : base(project)
		{
			// Todo: This is really ugly, fix logging
			//Initialized += MainWindow_Initialized; // doesn't work
			Activated += MainWindow_Activated;
		}

		private void MainWindow_Activated(object sender, EventArgs e)
		{
			Activated -= MainWindow_Activated;

			Load();
		}

		private void Load()
		{
			AddTab(new TabTest());
		}
	}
}

