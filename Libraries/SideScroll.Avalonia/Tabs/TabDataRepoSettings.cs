using SideScroll.Attributes;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Settings;
using SideScroll.Tabs.Tools.FileViewer;
using SideScroll.Tasks;
using SideScroll.Utilities;

namespace SideScroll.Avalonia.Tabs;

[PrivateData]
public class TabDataRepoSettings(UserSettings userSettings) : ITab
{
	public UserSettings UserSettings => userSettings;

	public TabInstance Create() => new Instance(this);

	private class Instance(TabDataRepoSettings tab) : TabInstance
	{
		protected DataSettings DataSettings => tab.UserSettings.DataSettings;

		public override void LoadUI(Call call, TabModel model)
		{
			model.AutoSelectSaved = AutoSelectType.NonEmpty;

			model.AddForm(DataSettings);

			List<ListItem> currentVersion =
			[
				new("App Directory", new TabDirectory(Project.Data.App.RepoPath)),
				new("Cache Directory", new TabDirectory(Project.Data.Cache.RepoPath)),
			];

			List<ListItem> allVersions =
			[
				new("App Directory", new TabDirectory(DataSettings.AppDataPath!)),
				new("Cache Directory", new TabDirectory(DataSettings.LocalDataPath!)),
				new("Shared Directory", new TabDirectory(Project.Data.Shared.RepoPath)),
				new("Exceptions", new TabDirectory(Project.ProjectSettings.ExceptionsPath)),
			];

			model.Items = new List<ListItem>
			{
				new("Current", currentVersion),
				new("All", allVersions),
			};

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Delete Repos - Current Version", DeleteRepos)
				{
					Flyout = new ConfirmationFlyoutConfig(
						"Are you sure you want to permanently delete the data repositories for the current version?",
						"Delete"),
					AccentType = AccentType.Warning,
				},
				new TaskDelegate("Delete Repos - All Versions", DeleteAllRepos)
				{
					Flyout = new ConfirmationFlyoutConfig(
						"Are you sure you want to permanently delete all data repositories across all versions?",
						"Delete"),
					AccentType = AccentType.Warning,
				}
			};
		}

		private void DeleteRepos(Call call)
		{
			Project.Data.Cache.DeleteRepo(call);
			Project.Data.App.DeleteRepo(call);
			Project.Data.Shared.DeleteRepo(call);

			Reload();
		}

		private void DeleteAllRepos(Call call)
		{
			FileUtils.DeleteDirectory(call, DataSettings.LocalDataPath);
			FileUtils.DeleteDirectory(call, DataSettings.AppDataPath);
			FileUtils.DeleteDirectory(call, Project.Data.Shared.RepoPath);
			FileUtils.DeleteDirectory(call, Project.ProjectSettings.ExceptionsPath);

			Reload();
		}
	}
}
