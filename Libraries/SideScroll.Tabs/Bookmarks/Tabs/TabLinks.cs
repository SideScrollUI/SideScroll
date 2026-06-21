using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Bookmarks.Tabs;

public class TabLinks : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var linkManager = LinkManager.Instance!;

			var items = new List<ListItem>
			{
				new("Imported", new TabLinkCollection(linkManager.Imported)),
				new("Created", new TabLinkCollection(linkManager.Created)),
			};
#if DEBUG
			items.Add(new("* Schema", new TabSchemas()));
#endif
			model.AddItems(items);
		}
	}
}
