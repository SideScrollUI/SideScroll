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
			Navigator.MaxHistorySize = _userSettings.MaxHistory;
		}
	}
	private UserSettings _userSettings;

	public Linker Linker { get; set; }

	public BookmarkNavigator Navigator { get; protected init; } = new();

	public ProjectDataRepos Data => new(ProjectSettings, UserSettings);

	[Obsolete("Use Data instead")]
	public DataRepo DataShared => Data.Shared;
	[Obsolete("Use Data instead")]
	public DataRepo DataApp => Data.App;
	[Obsolete("Use Data instead")]
	public DataRepo DataTemp => Data.Temp;

	public override string? ToString() => Name;

	public Project() : this(new ProjectSettings(), new UserSettings()) { }

	public Project(ProjectSettings projectSettings, UserSettings userSettings)
	{
		ProjectSettings = projectSettings;
		UserSettings = userSettings;
		_userSettings = userSettings; // Make the compiler happy
		Linker = new(this);
	}

	public void SaveUserSettings()
	{
		//DataApp.Save(UserSettings, new Call());

		var serializer = SerializerFile.Create(ProjectSettings.DefaultProjectPath);
		serializer.Save(new Call(), UserSettings);
	}

	public Project Open(LinkedBookmark linkedBookmark)
	{
		UserSettings userSettings = UserSettings.DeepClone()!;
		userSettings.LinkId = linkedBookmark.LinkId;
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
		defaultUserSettings ??= projectSettings.DefaultUserSettings as T ?? new()
		{
			ProjectPath = projectSettings.DefaultProjectPath,
		};
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
	public DataRepo App => new(AppPath, DataRepoName);
	public DataRepo Temp => new(TempPath, DataRepoName);
	public DataRepo Shared => new(SharedPath, DataRepoName); // Shared across versions

	private string AppPath => Paths.Combine(userSettings.ProjectPath, "Data", projectSettings.DataVersion.ToString());
	private string TempPath => Paths.Combine(userSettings.ProjectPath, "Temp", projectSettings.DataVersion.ToString());
	private string SharedPath => Paths.Combine(userSettings.ProjectPath, "Shared");

	private string DataRepoName
	{
		get
		{
			if (userSettings.LinkId != null)
			{
				return Paths.Combine("Links", userSettings.LinkId.HashSha256());
			}
			else
			{
				return Paths.Combine("Current");
			}
		}
	}
}
