using SideScroll.Resources;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SideScroll.Tabs.Settings;

/// <summary>
/// Contains project-wide configuration settings and metadata
/// </summary>
public class ProjectSettings
{
	/// <summary>
	/// Gets or sets the domain name for the project
	/// This is used to determine the save folder prefixes to use
	/// </summary>
	public string? Domain { get; set; }

	/// <summary>
	/// Gets or sets the project name
	/// This is used to determine the save folder prefixes to use
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the LinkUri type
	///  &lt;prefix&gt;://&lt;type&gt;/[v&lt;version&gt;/]&lt;path&gt;[?&lt;query&gt;]
	/// </summary>
	public string? LinkType { get; set; }

	/// <summary>
	/// Gets or sets whether linking functionality is enabled
	/// </summary>
	public bool EnableLinking { get; set; } = true;

	/// <summary>
	/// Gets or sets the project version
	/// This is used for links and what appears in the UI
	/// </summary>
	public Version Version { get; set; } = new();

	/// <summary>
	/// Gets or sets the data repository version
	/// Updating this will change the folder used to save local data
	/// Bump to current Version when making a breaking serialization change
	/// </summary>
	public Version DataVersion { get; set; } = new(); // What Data Repo version to use, bump to current Version when you make a breaking serialization change, (like a breaking NameSpace change, no renaming support yet)

	/// <summary>
	/// Gets or sets whether the toolbar is shown in the TabViewer
	/// </summary>
	public bool ShowToolbar { get; set; } = true;

	/// <summary>
	/// Gets or sets a custom title icon resource used in the TabViewer
	/// </summary>
	public IResourceView? CustomTitleIcon { get; set; }

	/// <summary>
	/// Gets whether custom titlebar is enabled by default on Windows and macOS platforms
	/// </summary>
	public static bool DefaultEnableCustomTitlebar => 
		RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
		RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	/// <summary>
	/// Gets the default application data path used by DataRepos
	/// </summary>
	public string DefaultAppDataPath => Paths.Combine(Paths.AppDataPath, RelativePath);

	/// <summary>
	/// Gets the default local data path based used by DataRepos
	/// </summary>
	public string DefaultLocalDataPath => Paths.Combine(Paths.LocalDataPath, RelativePath);

	/// <summary>
	/// Gets the relative path combining Domain and Name, or just Name if Domain is null
	/// </summary>
	public string RelativePath =>
		Domain != null
			? Paths.Combine(Domain, Name)
			: Name!;

	/// <summary>
	/// Gets the path where exception data is stored
	/// This is primarily used when a fatal exception occurs on the UI Thread
	/// </summary>
	public string ExceptionsPath => Paths.Combine(Paths.AppDataPath, RelativePath, "Exceptions");

	/// <summary>
	/// Gets the default user settings with pre-configured paths and platform-specific options
	/// </summary>
	public virtual UserSettings DefaultUserSettings => new()
	{
		EnableCustomTitleBar = DefaultEnableCustomTitlebar,
		DataSettings = new()
		{
			AppDataPath = DefaultAppDataPath,
			LocalDataPath = DefaultLocalDataPath,
		},
	};

	/// <summary>
	/// Gets the version of the entry assembly
	/// </summary>
	public static Version ProgramVersion() => Assembly.GetEntryAssembly()!.GetName().Version!;
}
