using SideScroll.Collections;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Tools.FileViewer;

namespace SideScroll.Tabs.Tools;

public class TabTools : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		private ItemCollectionUI<ListItem> _items = [];

		public override void Load(Call call, TabModel model)
		{
			model.Items = _items = new ItemCollectionUI<ListItem>
			{
				new("File Viewer", new TabFileViewer()),
				new("File Selector", new TabFileViewer(SelectFile)),
			};
		}

		public void SelectFile(Call call, string path)
		{
			string filename = Path.GetFileName(path);
			_items.Add(new ListItem(filename, new FileView(path)));
		}
	}
}
