using System.Runtime.Versioning;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;

namespace SideScroll.Demo.Avalonia.Browser;

/// <summary>
/// Browser-specific ProjectDataRepos that uses localStorage instead of file system
/// </summary>
[SupportedOSPlatform("browser")]
public class BrowserProjectDataRepos(ProjectSettings projectSettings, UserSettings userSettings) : ProjectDataRepos(projectSettings, userSettings)
{
	/// <summary>
	/// Gets a DataRepo for application-wide data using localStorage
	/// </summary>
	public override DataRepo App
	{
		get
		{
			Console.WriteLine($"🟡 BrowserProjectDataRepos.App accessed: {AppPath}");
			return new DataRepoLocalStorage(AppPath);
		}
	}

	/// <summary>
	/// Gets a DataRepo for cache data using localStorage
	/// </summary>
	public override DataRepo Cache
	{
		get
		{
			Console.WriteLine($"🟡 BrowserProjectDataRepos.Cache accessed: {CachePath}");
			return new DataRepoLocalStorage(CachePath);
		}
	}

	/// <summary>
	/// Gets a DataRepo for shared data using localStorage
	/// </summary>
	public override DataRepo Shared
	{
		get
		{
			Console.WriteLine($"🟡 BrowserProjectDataRepos.Shared accessed: {SharedPath}");
			return new DataRepoLocalStorage(SharedPath);
		}
	}
}
