﻿using Atlas.Core;
using Atlas.GUI.Wpf;
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
			Debug.Assert(SynchronizationContext.Current != null); // log needs this to work properly
			tabView = new TabView(new TabTest.Instance(project));
			tabView.Load();

			scrollViewer.Content = tabView;

			//LoadWindowSettings();
		}

		/* Avalonia's
		public static MainWindow LoadProject(string projectPath)
		{
			Project project = new Project(projectPath, typeof(MainWindow).Namespace);
			//this.settings = project.settings;

			MainWindow mainWindow = new MainWindow();
			mainWindow.tabView = new TabView(new TabAvalonia.Instance(project));
			//mainWindow.tabView = new TabView(new TabTest.Instance(project));
			mainWindow.tabView.LoadConfig();
			mainWindow.LoadProject(project);

			//Project project

			//scrollViewer.Content = tabView;
			return mainWindow;
		}*/
	}
}

