using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Settings;
using SideScroll.Time;

namespace SideScroll.Tabs;

[Unserialized]
public class Project
{
	public string? Name => ProjectSettings.Name;
	public string? LinkType => ProjectSettings.LinkType;

	public Version Version => ProjectSettings.Version;

	public virtual ProjectSettings ProjectSettings { get; set; }

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

	public virtual DataSettings DataSettings => UserSettings.DataSettings;

	public Linker Linker { get; set; }

	public BookmarkNavigator Navigator { get; protected init; } = new();

	public ProjectDataRepos Data => new(ProjectSettings, UserSettings);

	public override string? ToString() => Name;

	public Project() : this(new ProjectSettings(), new UserSettings()) { }

	public Project(ProjectSettings projectSettings, UserSettings userSettings)
	{
		ProjectSettings = projectSettings;
		userSettings.DataSettings.AppDataPath ??= projectSettings.DefaultAppDataPath;
		userSettings.DataSettings.LocalDataPath ??= projectSettings.DefaultLocalDataPath;
		UserSettings = userSettings;
		_userSettings = userSettings; // Make the compiler happy
		Linker = new(this);
	}

	public void SaveUserSettings()
	{
		//DataApp.Save(UserSettings, new Call());

		var serializer = SerializerFile.Create(ProjectSettings.DefaultAppDataPath);
		serializer.Save(new Call(), UserSettings);
	}

	public Project Open(LinkedBookmark linkedBookmark)
	{
		UserSettings userSettings = UserSettings.DeepClone()!;
		userSettings.DataSettings.LinkId = linkedBookmark.LinkId;
		var project = new Project(ProjectSettings, userSettings)
		{
			Linker = Linker,
		};
		//project.Import(bookmark);
		linkedBookmark.Bookmark.TabBookmark.Import(project);
		return project;
	}

	public static Project Load(ProjectSettings projectSettings, UserSettings? defaultUserSettings = null)
	{
		return Load<UserSettings>(projectSettings, defaultUserSettings);
	}

	public static Project Load<T>(ProjectSettings projectSettings, T? defaultUserSettings = null) where T : UserSettings, new()
	{
		defaultUserSettings ??= projectSettings.DefaultUserSettings as T ?? new();
		var project = new Project(projectSettings, defaultUserSettings);
		var userSettings = project.Data.App.Load<T>() ?? defaultUserSettings;
		return new Project(projectSettings, userSettings);
	}

	public void Initialize()
	{
		UserSettings.TimeZone ??= TimeZoneView.Local;
		TimeZoneView.Current = UserSettings.TimeZone;
		DateTimeExtensions.DefaultFormatType = UserSettings.TimeFormat;
		LinkManager.Instance = new(this);
	}
}

public class ProjectDataRepos(ProjectSettings projectSettings, UserSettings userSettings)
{
	protected DataSettings DataSettings => userSettings.DataSettings;

	public DataRepo App => new(AppPath);
	public DataRepo Cache => new(CachePath);
	public DataRepo Shared => new(SharedPath); // Shared across versions, can run into problems if schema changes

	public string AppPath => Paths.Combine(DataSettings.AppDataPath, "Data", projectSettings.DataVersion.ToString(), LinkPath);
	public string CachePath => Paths.Combine(DataSettings.LocalDataPath, "Cache", projectSettings.DataVersion.ToString(), LinkPath);
	public string SharedPath => Paths.Combine(DataSettings.AppDataPath, "Shared");

	protected string LinkPath =>
		DataSettings.LinkId is string linkId ?
			Paths.Combine("Links", linkId.HashSha256ToBase32()) :
			"Default";
}
