using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Tools;

public class TabFileBrowser : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance() : TabInstance
	{
		public const string GroupId = "Favorites";

		public DataRepoView<NodeView>? DataRepoNodes;

		public override void Load(Call call, TabModel model)
		{
			DataRepoNodes = Project.DataApp.LoadView<NodeView>(call, GroupId, nameof(NodeView.Name));
			foreach (var node in DataRepoNodes.Items.Values)
			{
				node.DataRepo = DataRepoNodes;
			}

			model.Items = new List<ListItem>()
			{
				new("Current", new TabDirectory(Directory.GetCurrentDirectory(), DataRepoNodes)),
				new("Downloads", new TabDirectory(Paths.DownloadPath, DataRepoNodes)),
				new("Drives", new TabDrives(DataRepoNodes)),
				new("Favorites", new TabFileFavorites(DataRepoNodes)),
			};
		}
	}
}
