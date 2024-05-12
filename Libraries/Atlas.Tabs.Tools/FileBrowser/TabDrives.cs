using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Tools;

public class TabDrives(DataRepoView<NodeView>? dataRepoFavorites = null) : ITab
{
	public DataRepoView<NodeView>? DataRepoFavorites = dataRepoFavorites;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabDrives tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			DriveInfo[] drives = DriveInfo.GetDrives();

			model.Items = drives
				.Select(d => new TabDirectory(d.Name, tab.DataRepoFavorites))
				.ToList();
		}
	}
}
