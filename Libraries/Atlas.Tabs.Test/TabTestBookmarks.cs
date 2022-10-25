using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Test;

public class TabTestBookmarks : ITab
{
	public override string ToString() => "Bookmarks";

	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			//model.Items = Project.Navigator.History;

			BookmarkNavigator navigator = Project.Navigator.DeepClone(call)!; // Clone from UI thread only
			navigator.History.RemoveAt(navigator.History.Count - 1); // remove the current in progress bookmark
			navigator.CurrentIndex = navigator.History.Count;

			model.Items = new List<ListItem>()
			{
				new("Navigator (snapshot)", navigator),
				//new("Recursive Tab", new TabSample()),
			};

			model.Notes = "The Navigator class creates a bookmark for every tab change you make, and allows you to move backwards and forwards. The Back/Forward buttons currently use this. Eventually a list/drop down could be used to select the bookmark";
		}
	}
}
