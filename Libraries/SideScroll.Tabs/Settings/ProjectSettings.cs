using SideScroll;
using System.Reflection;

namespace SideScroll.Tabs;

public class ProjectSettings
{
	public string? Domain { get; set; }
	public string? Name { get; set; }

	public string? LinkType { get; set; } // for bookmarking
	public bool EnableLinking { get; set; } = true;

	public Version Version { get; set; } = new();
	public Version DataVersion { get; set; } = new(); // What Data Repo version to use, bump to current Version when you make a breaking serialization change, (like a breaking NameSpace change, no renaming support yet)

	public bool ShowToolbar { get; set; } = true;

	public string DefaultProjectPath
	{
		get
		{
			if (Domain != null)
				return Paths.Combine(Paths.AppDataPath, Domain, Name);
			else
				return Paths.Combine(Paths.AppDataPath, Name);
		}
	}

	public static Version ProgramVersion() => Assembly.GetEntryAssembly()!.GetName().Version!;
}
