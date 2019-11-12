using System;
using System.Threading;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.Console
{
	public class Console
	{
		private CancellationTokenSource tokenSource = new CancellationTokenSource();
		private CancellationToken cancellationToken;

		public Project project;
		public Call call;
		public ProjectSettings settings = new ProjectSettings();
		public LogWriterConsole logWriterConsole;
		public LogWriterText logWriterText;

		static void Main(string[] args)
		{
			Console console = new Console();
		}

		public Console()
		{
			// setup
			Project project = LoadProject(UserSettings.DefaultProjectPath);
			call = new Call(GetType().Name);
			logWriterConsole = new LogWriterConsole(call.log);
			logWriterText = new LogWriterText(call.log, project.DataApp.GetTypePath(typeof(Console)) + "/Logs/Main");
			cancellationToken = tokenSource.Token;

			//for (int i = 0; i < 100; i++)
			//	TestDictionarySpeed();
			//TestLogWriter();
		}

		public static Project LoadProject(string projectPath)
		{
			var projectSettings = new ProjectSettings()
			{
				Name = "Atlas",
				Version = "1",
				DataVersion = "1",
				LinkType = "atlas",
			};
			var userSettings = new UserSettings()
			{
				ProjectPath = projectPath,
			};
			Project project = new Project(projectSettings, userSettings);
			return project;
		}

		public string GetProjectPath()
		{
			string projectPath = UserSettings.DefaultProjectPath;
			/*if (projectPath == null || projectPath.Length == 0)
			{
				System.Console.Write("Enter Project Path: ");
				projectPath = System.Console.ReadLine();
				ProjectSettings.DefaultProjectPath = projectPath;
			}*/
			return projectPath;
		}

		void TestLogWriter()
		{
			call.log.Add("test");
		}
	}
}
/*
This should probably get turned into a library if it ever gets used
*/
