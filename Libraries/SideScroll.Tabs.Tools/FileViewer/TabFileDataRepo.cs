using SideScroll.Resources;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Tools.FileViewer;

public class TabFileDataRepo(DataRepoView<NodeView> dataRepoNodes, FileSelectorOptions? fileSelectorOptions = null) : ITab
{
	public DataRepoView<NodeView> DataRepoNodes => dataRepoNodes;
	public FileSelectorOptions? FileSelectorOptions { get; set; } = fileSelectorOptions;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonClearAll { get; set; } = new("Clear All", Icons.Svg.DeleteList);
	}

	public class Instance(TabFileDataRepo tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Editing = true;

			Toolbar toolbar = new();
			toolbar.ButtonClearAll.Action = ClearAll;
			model.AddObject(toolbar);

			tab.DataRepoNodes.LoadAllIndexed(call);
			List<NodeView> nodeViews = tab.DataRepoNodes.Items.Values.ToList();
			foreach (var node in nodeViews)
			{
				node.FileSelectorOptions = tab.FileSelectorOptions;
			}

			model.AddData(nodeViews);
		}

		private void ClearAll(Call call)
		{
			tab.DataRepoNodes.DeleteAll(call);
			Reload();
		}
	}
}
