﻿using Atlas.Tabs;
using System;
using System.Windows;

namespace Atlas.Start.Wpf
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			/*if (projectPath == null || projectPath.Length == 0)
			{
				Content = new NewProject(this);
				return;
			}*/

			Project project = LoadProject(UserSettings.DefaultProjectPath);
			MainWindow window = new MainWindow(project);
			//window.LoadSettings(Settings.DefaultProjectPath);
			window.Show();
		}

		public static Project LoadProject(string projectPath)
		{
			var projectSettings = new ProjectSettings()
			{
				Name = "Atlas",
				LinkType = "atlas",
				Version = new Version(1, 0),
				DataVersion = new Version(1, 0),
			};
			var userSettings = new UserSettings()
			{
				ProjectPath = projectPath,
			};
			Project project = new Project(projectSettings, userSettings);
			return project;
		}

		public void ShowNewProject()
		{
			NewProject newProject = new NewProject(this);
		}
	}
}
