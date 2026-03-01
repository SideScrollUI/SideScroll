using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Settings;
using SideScroll.Time;

namespace SideScroll.Tabs;

/// <summary>
/// Represents a SideScroll project with settings, navigation, data repositories, and bookmark linking support
/// </summary>
[Unserialized]
public class Project
{
	/// <summary>
	/// Gets the project name from project settings
	/// </summary>
	public string? Name => ProjectSettings.Name;

	/// <summary>
	/// Gets the link type from project settings
	/// </summary>
	public string? LinkType => ProjectSettings.LinkType;

	/// <summary>
	/// Gets the project version from project settings
	/// </summary>
	public Version Version => ProjectSettings.Version;

	/// <summary>
	/// Gets or sets the project settings
	/// </summary>
	public virtual ProjectSettings ProjectSettings { get; set; }

	/// <summary>
	/// Gets or sets the user settings, updating navigation history size when changed
	/// </summary>
	public virtual UserSettings UserSettings
	{
		get => _userSettings;
		set
		{
			_userSettings = value;
			Navigator.MaxHistorySize = _userSettings.DataSettings.MaxHistory;
		}
	}
	private UserSettings _userSettings;

	/// <summary>
	/// Gets the data settings from user settings
	/// </summary>
	public virtual DataSettings DataSettings => UserSettings.DataSettings;

	/// <summary>
	/// Gets or sets the bookmark linker for creating and retrieving links
	/// </summary>
	public Linker Linker { get; set; }

	/// <summary>
	/// Gets the bookmark navigator for managing navigation history
	/// </summary>
	public BookmarkNavigator Navigator { get; protected init; } = new();

	/// <summary>
	/// Gets the data repositories accessor for app, cache, and shared data
	/// </summary>
	public ProjectDataRepos Data => new(ProjectSettings, UserSettings);

	public override string? ToString() => Name;

	/// <summary>
	/// Initializes a new project with default settings
	/// </summary>
	public Project() : this(new ProjectSettings(), new UserSettings()) { }

	/// <summary>
	/// Initializes a new project with the specified project and user settings
	/// </summary>
	public Project(ProjectSettings projectSettings, UserSettings userSettings)
	{
		ProjectSettings = projectSettings;
		userSettings.EnableCustomTitleBar ??= ProjectSettings.DefaultEnableCustomTitlebar;
		userSettings.DataSettings.AppDataPath ??= projectSettings.DefaultAppDataPath;
		userSettings.DataSettings.LocalDataPath ??= projectSettings.DefaultLocalDataPath;
		UserSettings = userSettings;
		_userSettings = userSettings; // Make the compiler happy
		Linker = new(this);
	}

	/// <summary>
	/// Saves the user settings to the default app data path
	/// </summary>
	public void SaveUserSettings()
	{
		//DataApp.Save(UserSettings, new Call());

		var serializer = SerializerFile.Create(ProjectSettings.DefaultAppDataPath);
		serializer.Save(new Call(), UserSettings);
	}

	/// <summary>
	/// Opens a new project instance for a linked bookmark with imported data
	/// </summary>
	public Project Open(LinkedBookmark linkedBookmark)
	{
		UserSettings userSettings = UserSettings.DeepClone();
		userSettings.DataSettings.LinkId = linkedBookmark.LinkId;
		var project = new Project(ProjectSettings, userSettings)
		{
			Linker = Linker,
		};
		//project.Import(bookmark);
		linkedBookmark.Bookmark.Import(project);
		return project;
	}

	/// <summary>
	/// Loads a project from saved user settings or uses defaults
	/// </summary>
	public static Project Load(ProjectSettings projectSettings, UserSettings? defaultUserSettings = null)
	{
		return Load<UserSettings>(projectSettings, defaultUserSettings);
	}

	/// <summary>
	/// Loads a project with custom user settings type from saved data or uses defaults
	/// </summary>
	public static Project Load<T>(ProjectSettings projectSettings, T? defaultUserSettings = null) where T : UserSettings, new()
	{
		defaultUserSettings ??= projectSettings.DefaultUserSettings as T ?? new();
		var project = new Project(projectSettings, defaultUserSettings);
		var userSettings = project.Data.App.Load<T>() ?? defaultUserSettings;
		return new Project(projectSettings, userSettings);
	}

	/// <summary>
	/// Initializes global settings including time zone, date format, and link manager
	/// </summary>
	public void Initialize()
	{
		TimeZoneView.Current = UserSettings.TimeZone;
		DateTimeExtensions.DefaultFormatType = UserSettings.TimeFormat;
		LinkManager.Instance = new(this);
	}
}

/// <summary>
/// Provides access to project data repositories for app data, cache, and shared data across versions
/// </summary>
public class ProjectDataRepos(ProjectSettings projectSettings, UserSettings userSettings)
{
	/// <summary>
	/// Gets the data settings from user settings
	/// </summary>
	public DataSettings DataSettings => userSettings.DataSettings;

	/// <summary>
	/// Gets the app data repository
	/// </summary>
	public DataRepo App => new(AppPath);

	/// <summary>
	/// Gets the cache data repository
	/// </summary>
	public DataRepo Cache => new(CachePath);

	/// <summary>
	/// Gets the shared data repository (shared across versions, may have schema compatibility issues)
	/// </summary>
	public DataRepo Shared => new(SharedPath);

	/// <summary>
	/// Gets the app data path including version and link-specific subdirectories
	/// </summary>
	public string AppPath => Paths.Combine(DataSettings.AppDataPath, "Data", projectSettings.DataVersion.ToString(), LinkPath);

	/// <summary>
	/// Gets the cache data path including version and link-specific subdirectories
	/// </summary>
	public string CachePath => Paths.Combine(DataSettings.LocalDataPath, "Cache", projectSettings.DataVersion.ToString(), LinkPath);

	/// <summary>
	/// Gets the shared data path (not versioned)
	/// </summary>
	public string SharedPath => Paths.Combine(DataSettings.AppDataPath, "Shared");

	/// <summary>
	/// Gets the link-specific path component, either a hashed link ID or "Default"
	/// </summary>
	protected string LinkPath =>
		DataSettings.LinkId is string linkId ?
			Paths.Combine("Links", linkId.HashSha256ToBase32()) :
			"Default";
}
