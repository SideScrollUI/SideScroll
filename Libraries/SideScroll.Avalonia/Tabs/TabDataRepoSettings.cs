using SideScroll.Attributes;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Tools.FileViewer;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Tabs;

[PrivateData]
public class TabDataRepoSettings : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.AutoSelectSaved = AutoSelectType.NonEmpty;

			List<ListItem> currentVersion =
			[
				new("App Directory", new TabDirectory(Project.Data.App.RepoPath)),
				new("Cache Directory", new TabDirectory(Project.Data.Cache.RepoPath)),
			];

			List<ListItem> allVersions =
			[
				new("App Directory", new TabDirectory(Project.UserSettings.AppDataPath!)),
				new("Cache Directory", new TabDirectory(Project.UserSettings.LocalDataPath!)),
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
			Directory.Delete(Project.UserSettings.LocalDataPath!, true);
			Directory.Delete(Project.UserSettings.AppDataPath!, true);
			Directory.Delete(Project.Data.Shared.RepoPath, true);
			Directory.Delete(Project.ProjectSettings.ExceptionsPath, true);

			Reload();
		}
	}
}
