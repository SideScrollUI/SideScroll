using Atlas.Core;

namespace Atlas.Tabs
{
	public class ProjectSettings
	{
		public string Name { get; set; }
		public string LinkType { get; set; }        // for bookmarking
		public string Version { get; set; } = "0";
		public string DataVersion { get; set; } = "0"; // What Data Repo version to use, bump to current Version when you make a breaking serialization change, (like a breaking NameSpace change, no renaming support yet)
	}
}