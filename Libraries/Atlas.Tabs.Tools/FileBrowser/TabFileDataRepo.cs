using Atlas.Core;
using Atlas.Resources;
using Atlas.Serialize;
using Atlas.Tabs.Toolbar;

namespace Atlas.Tabs.Tools;

public class TabFileDataRepo(DataRepoView<NodeView> dataRepoNodes) : ITab
{
	public DataRepoView<NodeView> DataRepoNodes = dataRepoNodes;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonClearAll { get; set; } = new("Clear All", Icons.Svg.DeleteList);
	}

	public class Instance(TabFileDataRepo tab) : TabInstance, ITabAsync
	{
		public async Task LoadAsync(Call call, TabModel model)
		{
			model.Editing = true;

			Toolbar toolbar = new();
			toolbar.ButtonClearAll.Action = ClearAll;
			model.AddObject(toolbar);

			tab.DataRepoNodes.LoadAllIndexed(call);
			List<NodeView> nodeViews = tab.DataRepoNodes.Items.Values.ToList();
			if (nodeViews.Count > 0)
			{
				DataRepoView<NodeView> fileFavorites = await FileDataRepos.Favorites.OpenViewAsync(Project);
				foreach (var node in nodeViews)
				{
					node.DataRepoFavorites = fileFavorites;
				}
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
