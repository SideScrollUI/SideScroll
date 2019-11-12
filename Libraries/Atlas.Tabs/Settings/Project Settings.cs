using Atlas.Core;

namespace Atlas.Tabs
{
	public class ProjectSettings
	{
		public string Name { get; set; }
		public string LinkType { get; set; }        // for bookmarking
		public string Version { get; set; } = "0";
		public string DataVersion { get; set; } = "0";
	}
}