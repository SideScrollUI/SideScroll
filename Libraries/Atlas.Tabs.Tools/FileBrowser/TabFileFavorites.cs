using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Tools;

public class TabFileFavorites(DataRepoView<NodeView> dataRepoNodes) : ITab
{
	public DataRepoView<NodeView> DataRepoNodes = dataRepoNodes;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabFileFavorites tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Editing = true;

			model.AddData(tab.DataRepoNodes.Items.Values);
		}
	}
}
