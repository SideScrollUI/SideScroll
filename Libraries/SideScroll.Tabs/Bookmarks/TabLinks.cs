using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Bookmarks;

public class TabLinks : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var linkManager = LinkManager.Instance!;

			model.Items = new List<ListItem>
			{
				new("Imported", new TabLinkCollection(linkManager.Imported)),
				new("Created", new TabLinkCollection(linkManager.Created)),
			};
		}
	}
}
