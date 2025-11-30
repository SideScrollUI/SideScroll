using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using System.Collections;

namespace SideScroll.Tabs.Tools.FileViewer;

[PrivateData]
public class TabFileDataRepo(DataRepoView<NodeView> dataRepoNodes, FileSelectorOptions? fileSelectorOptions = null) : ITab
{
	public DataRepoView<NodeView> DataRepoNodes => dataRepoNodes;
	public FileSelectorOptions? FileSelectorOptions { get; set; } = fileSelectorOptions;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonClearAll { get; } = new("Clear All", Icons.Svg.DeleteList)
		{
			Flyout = new ConfirmationFlyoutConfig("Clear All?", "Confirm"),
		};
	}

	public class Instance(TabFileDataRepo tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Editing = true;

			Toolbar toolbar = new();
			toolbar.ButtonClearAll.IsEnabledBinding = new PropertyBinding(nameof(IList.Count), tab.DataRepoNodes.Items);
			toolbar.ButtonClearAll.Action = ClearAll;
			model.AddObject(toolbar);

			tab.DataRepoNodes.LoadAllIndexed(call);
			List<NodeView> nodeViews = tab.DataRepoNodes.Values.ToList();
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
