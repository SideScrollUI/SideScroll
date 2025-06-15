using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Tools.FileViewer;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Tabs;

public class TabDataRepoSettings : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.AutoSelectSaved = AutoSelectType.NonEmpty;

			model.Items = new List<ListItem>
			{
				new("App Directory", new TabDirectory(Project.Data.App.RepoPath)),
				new("Shared Directory", new TabDirectory(Project.Data.Shared.RepoPath)),
				new("Temp Directory", new TabDirectory(Project.Data.Temp.RepoPath)),
			};

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Delete Data Repos", DeleteRepos)
				{
					Flyout = new ConfirmationFlyoutConfig("Are you sure you want to delete all Data Repos?", "Delete"),
					AccentType = AccentType.Warning,
				}
			};
		}

		private void DeleteRepos(Call call)
		{
			Project.Data.Temp.DeleteRepo(call);
			Project.Data.App.DeleteRepo(call);
			Project.Data.Shared.DeleteRepo(call);
			Reload();
		}
	}
}
