using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Samples.Forms;
using SideScroll.Tabs.Tools.FileViewer;

namespace SideScroll.Tabs.Samples.DataRepo;

public class TabSampleDataRepo : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
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
		}
	}
}
