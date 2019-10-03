using Atlas.Core;

namespace Atlas.Tabs
{
	public class ProjectSettings
	{
		private static string _DefaultProjectPath;
		public static string DefaultProjectPath
		{
			get
			{
				if (_DefaultProjectPath == null)
				{
					_DefaultProjectPath = Paths.Combine(Paths.AppDataPath, "Atlas");
				}
				return _DefaultProjectPath;
			}
			set
			{
				_DefaultProjectPath = value;

			}
		}

		public string Name { get; set; }
		public string LinkType { get; set; }        // for bookmarking
		public string Version { get; set; } = "0";
		public string DataVersion { get; set; } = "0";

		public string ProjectPath { get; set; }

		public string SettingsPath { get { return GetSettingsPath(ProjectPath); } }

		//public const bool Reset = false;
		public bool AutoLoad = true;

		public int SubTabLimit { get; set; } = 10;

		//public int MaxLogItems { get; set; } = 100000;

		public static string GetSettingsPath(string projectPath) { return Paths.Combine(projectPath, @"Settings.atlas"); }
	}
}