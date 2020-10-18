using Atlas.Core;
using Atlas.Tabs.Tools;

//namespace Atlas.Tabs.Test.DataRepo // good idea?
namespace Atlas.Tabs.Test
{
	public class TabTestDataRepo : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample Data Repo", new TabTestDataRepoCollection()),
					new ListItem("Local Directories", new TabDirectory(Project.DataApp.RepoPath)),
				};

				model.Actions =  new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Delete Repos", DeleteRepos),
				};
				model.AutoSelect = TabModel.AutoSelectType.AnyNewOrSaved;

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

}
