using SideScroll.Core;

namespace SideScroll.Tabs.Tools;

public class TabTools : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private ItemCollectionUI<ListItem> _items = [];

		public override void Load(Call call, TabModel model)
		{
			model.Items = _items = new ItemCollectionUI<ListItem>
			{
				new("File Browser", new TabFileBrowser()),
				new("File Selector", new TabFileBrowser(SelectFile)),
			};
		}

		public void SelectFile(Call call, string path)
		{
			string filename = Path.GetFileName(path);
			_items.Add(new ListItem(filename, new FileView(path)));
		}
	}
}
