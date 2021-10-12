using System;
using System.Threading;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.Console
{
	public class Console
	{
		public Project project;
		public Call call;
		public ProjectSettings settings = new ProjectSettings();
		public LogWriterConsole logWriterConsole;
		public LogWriterText logWriterText;

		static void Main(string[] args)
		{
			var console = new Console();
		}

		public Console()
		{
			// setup
			var project = new Project(Settings);
			call = new Call(GetType().Name);
			logWriterConsole = new LogWriterConsole(call.Log);
			logWriterText = new LogWriterText(call.Log, project.DataApp.GetGroupPath(typeof(Console)) + "/Logs/Main");

			//TestLogWriter();
		}

		public static ProjectSettings Settings => new ProjectSettings()
		{
			Name = "Atlas",
			LinkType = "atlas",
			Version = new Version(1, 0),
			DataVersion = new Version(1, 0),
		};

		void TestLogWriter()
		{
			call.Log.Add("test");
		}
	}
}
/*
This should probably get turned into a library if it ever gets used
*/
