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
	/// Loads project with default settings for browser
	/// Settings will be loaded asynchronously after storage.js module is imported
	/// </summary>
	public static BrowserProject Load()
	{
		var projectSettings = SampleProjectSettings.Settings;
		var userSettings = new SampleUserSettings();
		var project = new BrowserProject(projectSettings, userSettings);
		project.Initialize();
		return project;
	}

	/// <summary>
	/// Asynchronously loads project with user settings from localStorage
	/// Call this after storage.js module has been imported
	/// </summary>
	public static async Task<BrowserProject> LoadAsync()
	{
		var projectSettings = SampleProjectSettings.Settings;
		var defaultUserSettings = new SampleUserSettings();
		var tempProject = new BrowserProject(projectSettings, defaultUserSettings);
		
		// Use the standard pattern: project.Data.App.Load<T>()
		var userSettings = tempProject.Data.App.Load<SampleUserSettings>() ?? defaultUserSettings;
		
		var project = new BrowserProject(projectSettings, userSettings);
		project.Initialize();
		
		// If no saved settings existed, save the defaults
		if (userSettings == defaultUserSettings)
		{
			project.Data.App.Save(userSettings);
		}
		
		return project;
	}
}

