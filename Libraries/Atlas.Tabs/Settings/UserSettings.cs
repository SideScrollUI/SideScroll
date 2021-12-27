using Atlas.Core;

namespace Atlas.Tabs
{
	public class UserSettings
	{
		public string ProjectPath { get; set; }
		public string BookmarkPath { get; set; }

		public string SettingsPath => Paths.Combine(ProjectPath, @"Settings.atlas");

		public bool AutoLoad { get; set; } = true;

		public int VerticalTabLimit { get; set; } = 10;

		//public int MaxLogItems { get; set; } = 100000;
	}
}
