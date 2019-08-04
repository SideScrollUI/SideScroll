using System;
using System.Collections.Generic;
using Atlas.Core;
using Atlas.Tabs.Tools;

//namespace Atlas.Tabs.Test.DataRepo // good idea?
namespace Atlas.Tabs.Test
{
	public class TabTestDataRepo : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample Data Repo", new TabTestDataRepoCollection()),
					new ListItem("Local Directories", new TabDirectory(project.DataApp.RepoPath)),
				};

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Delete Repos", DeleteRepos),
				};

				tabModel.Notes = "Data Repos store C# objects as serialized data.";
			}

			private void DeleteRepos(Call call)
			{
				project.DataShared.DeleteRepo();
				project.DataApp.DeleteRepo();
				Reload();
			}
		}
	}

}
