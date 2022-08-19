using Atlas.Core;
using Atlas.Tabs.Tools;

namespace Atlas.Tabs.Test.DataRepo;

public class TabTestDataRepo : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>()
			{
				new("Sample Data Repo", new TabTestDataRepoCollection()),
				new("Local Directories", new TabDirectory(Project.DataApp.RepoPath)),
			};

			model.Actions = new List<TaskCreator>()
			{
				new TaskDelegate("Delete Repos", DeleteRepos),
			};
			model.AutoSelect = AutoSelectType.AnyNewOrSaved;

			model.Notes = "Data Repos store C# objects as serialized data.";
		}

		private void DeleteRepos(Call call)
		{
			Project.DataShared.DeleteRepo();
			Project.DataApp.DeleteRepo();
			Reload();
		}
	}
}
