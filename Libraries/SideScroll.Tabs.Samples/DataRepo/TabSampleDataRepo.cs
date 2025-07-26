using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Samples.Forms;
using SideScroll.Tabs.Tools.FileViewer;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Samples.DataRepo;

public class TabSampleDataRepo : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.AutoSelectSaved = AutoSelectType.NonEmpty;

			model.Items = new List<ListItem>
			{
				new("Sample Data Repo", new TabSampleDataRepoCollection()),
				new("Param Data Repo", new TabSampleFormDataTabs()),
				new("Paging", new TabSampleDataRepoPaging()),
				new("App Directory", new TabDirectory(Project.Data.App.RepoPath)),
				new("Cache Directory", new TabDirectory(Project.Data.Cache.RepoPath)),
			};

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Delete Repos", DeleteRepos)
				{
					Flyout = new ConfirmationFlyoutConfig("Are you sure you want to delete all DataRepos?", "Delete"),
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
	}
}
