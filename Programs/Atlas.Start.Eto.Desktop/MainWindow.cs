using System;
using Eto.Forms;
using Eto.Drawing;
using Atlas.Core;
using Atlas.Serialize;
using Atlas.Tabs.Test;
using Atlas.Tabs;
using Atlas.GUI.Eto;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;

namespace Atlas.Start.Eto.Desktop
{
	public class MainWindow : BaseWindow
	{
		public MainWindow(Project project) : base(project)
		{
			// Todo: This is really ugly, fix logging
			//this.Initialized += MainWindow_Initialized; // doesn't work
			LoadProject();
		}

		public MainWindow(string projectPath) : base(new Project(projectPath, typeof(MainWindow).Namespace))
		{
			// Todo: This is really ugly, fix logging (needs a GUI context to work)
			//this.Initialized += MainWindow_Initialized; // doesn't work
			LoadProject();
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			// SynchronizationContext.Current isn't valid until form is shown, and it gets attached to the Log
			//Content = CreateView();
		}

		public void LoadProject() // Load() already used by base
		{
			//tabView = new TabView(new TabEto.Instance(project));
			tabView = new TabView(new TabTest.Instance(project));
			tabView.LoadConfig();

			Content = tabView;
			//scrollViewer.Content = tabView;
		}
	}
}