using System.Reflection;

namespace SideScroll.Tabs.Settings;

public class ProjectSettings
{
	public string? Domain { get; set; }
	public string? Name { get; set; }

	public string? LinkType { get; set; } // for bookmarking
	public bool EnableLinking { get; set; } = true;

	public Version Version { get; set; } = new();
	public Version DataVersion { get; set; } = new(); // What Data Repo version to use, bump to current Version when you make a breaking serialization change, (like a breaking NameSpace change, no renaming support yet)

	public bool ShowToolbar { get; set; } = true;

	public string DefaultAppDataPath => Paths.Combine(Paths.AppDataPath, RelativePath);

	public string DefaultLocalDataPath => Paths.Combine(Paths.LocalDataPath, RelativePath);

	public string RelativePath =>
		Domain != null
			? Paths.Combine(Domain, Name)
			: Name!;

	public virtual UserSettings DefaultUserSettings => new()
	{
		AppDataPath = DefaultAppDataPath,
		LocalDataPath = DefaultLocalDataPath,
	};

	public static Version ProgramVersion() => Assembly.GetEntryAssembly()!.GetName().Version!;
}
