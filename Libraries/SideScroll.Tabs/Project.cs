using SideScroll.Extensions;
using SideScroll.Network.Http;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Settings;
using SideScroll.Tasks;

namespace SideScroll.Tabs;

public class Project
{
	public string? Name => ProjectSettings.Name; // for viewing purposes
	public string? LinkType => ProjectSettings.LinkType;
	public Version Version => ProjectSettings.Version;
	public virtual ProjectSettings ProjectSettings { get; set; }
	public virtual UserSettings UserSettings { get; set; } = new();

	public Linker Linker { get; set; }

	public DataRepo DataShared => new(DataSharedPath, DataRepoName);
	public DataRepo DataApp => new(DataAppPath, DataRepoName);
	public DataRepo DataTemp => new(DataTempPath, DataRepoName);

	public HttpCacheManager Http = new();
	public BookmarkNavigator Navigator { get; set; } = new();
	public TaskInstanceCollection Tasks { get; set; } = [];

	private string DataSharedPath => Paths.Combine(UserSettings.ProjectPath, "Shared");
	private string DataAppPath => Paths.Combine(UserSettings.ProjectPath, "Versions", ProjectSettings.DataVersion.ToString()); // todo: Rename Version to Data next schema change
	private string DataTempPath => Paths.Combine(UserSettings.ProjectPath, "Temp", ProjectSettings.DataVersion.ToString());

	private string DataRepoName
	{
		get
		{
			if (UserSettings.BookmarkPath != null)
			{
				return Paths.Combine("Bookmarks", UserSettings.BookmarkPath.HashSha256());
			}
			else
			{
				return Paths.Combine("Current");
			}
		}
	}

	public override string? ToString() => Name;

	public Project()
	{
		ProjectSettings = new();
		Linker = new(this);
	}

	public Project(ProjectSettings projectSettings)
	{
		ProjectSettings = projectSettings;
		UserSettings = new UserSettings
		{
			ProjectPath = projectSettings.DefaultProjectPath,
		};
		// Todo: Improve this
		UserSettings = DataApp.Load<UserSettings>() ?? UserSettings;
		Linker = new(this);
	}

	public Project(ProjectSettings projectSettings, UserSettings userSettings)
	{
		ProjectSettings = projectSettings;
		UserSettings = userSettings;
		Linker = new(this);
	}

	public void SaveSettings()
	{
		//DataApp.Save(projectSettings, new Call());

		var serializer = SerializerFile.Create(UserSettings.SettingsPath, "");
		serializer.Save(new Call(), ProjectSettings);
	}

	public Project Open(Bookmark bookmark)
	{
		UserSettings userSettings = UserSettings.DeepClone()!;
		userSettings.BookmarkPath = bookmark.Path;
		var project = new Project(ProjectSettings, userSettings)
		{
			Linker = Linker,
		};
		//project.Import(bookmark);
		bookmark.TabBookmark.Import(project);
		return project;
	}

	public static Project Load<T>(ProjectSettings projectSettings) where T: UserSettings, new()
	{
		T defaultUserSettings = new()
		{
			ProjectPath = projectSettings.DefaultProjectPath,
		};
		var project = new Project(projectSettings, defaultUserSettings);
		var userSettings = project.DataApp.Load<T>() ?? defaultUserSettings;
		return new Project(projectSettings, userSettings);
	}
}
