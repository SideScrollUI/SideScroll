using System.Runtime.Versioning;
using SideScroll.Avalonia.Samples;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;

namespace SideScroll.Demo.Avalonia.Browser;

/// <summary>
/// Browser-specific Project that uses localStorage for data persistence
/// </summary>
[SupportedOSPlatform("browser")]
public class BrowserProject(ProjectSettings projectSettings, UserSettings userSettings)
	: Project(projectSettings, userSettings)
{
	/// <summary>
	/// Override Data property to use BrowserProjectDataRepos with localStorage
	/// </summary>
	public override ProjectDataRepos Data => new BrowserProjectDataRepos(ProjectSettings, UserSettings);

	/// <summary>
	/// Loads project with settings restored from localStorage.
	/// </summary>
	public static BrowserProject Load()
	{
		var projectSettings = SampleProjectSettings.Settings;
		var defaultUserSettings = new SampleUserSettings();

		// Create a temporary project to access Data.App for loading previously saved UserSettings
		var tempProject = new BrowserProject(projectSettings, defaultUserSettings);
		var userSettings = tempProject.Data.App.Load<SampleUserSettings>() ?? defaultUserSettings;

		return new BrowserProject(projectSettings, userSettings);
	}
}
