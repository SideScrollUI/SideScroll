using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Samples.Params;
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
				new("Param Data Repo", new TabSampleParamsDataTabs()),
				new("Paging", new TabSampleDataRepoPaging()),
				new("Local Directories", new TabDirectory(Project.DataApp.RepoPath)),
			};

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Delete Repos", DeleteRepos),
			};
		}

		private void DeleteRepos(Call call)
		{
			Project.DataShared.DeleteRepo(call);
			Project.DataApp.DeleteRepo(call);
			Reload();
		}
	}
}
