using Atlas.Core;

namespace Atlas.Tabs
{
	public class UserSettings
	{
		public static string DefaultProjectPath => Paths.Combine(Paths.AppDataPath, "Atlas");

		public static string GetSettingsPath(string projectPath) { return Paths.Combine(projectPath, @"Settings.atlas"); }

		public string ProjectPath { get; set; }

		public string SettingsPath { get { return GetSettingsPath(ProjectPath); } }

		//public const bool Reset = false;
		public bool AutoLoad = true;

		public int SubTabLimit { get; set; } = 10;

		//public int MaxLogItems { get; set; } = 100000;
	}
}