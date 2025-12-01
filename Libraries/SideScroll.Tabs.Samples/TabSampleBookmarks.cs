using SideScroll.Attributes;
using SideScroll.Serialize;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples;

[TabRoot, PublicData]
public class TabSampleBookmarks : ITab
{
	public override string ToString() => "Bookmarks";

	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			BookmarkNavigator navigator = Project.Navigator.DeepClone(call); // Clone from UI thread only
			navigator.History.RemoveAt(navigator.History.Count - 1); // Remove the current in progress bookmark
			navigator.CurrentIndex = navigator.History.Count;

			model.Items = new List<ListItem>
			{
				new("Navigator (snapshot)", navigator),
			};
		}
	}
}
