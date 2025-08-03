using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tools.FileViewer;

public class TabFileBytes(string path) : ITab
{
	public string Path => path;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabFileBytes tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = ListByte.Load(tab.Path);
		}
	}
}
