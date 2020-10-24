using Atlas.Core;
using Atlas.Serialize;
using System.Collections.Generic;

namespace Atlas.Tabs.Test
{
	public class TabTestBookmarks : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				//tabModel.Items = project.navigator.History;

				BookmarkNavigator navigator = Project.Navigator.DeepClone(call);
				navigator.History.RemoveAt(navigator.History.Count - 1); // remove the current in progress bookmark
				navigator.CurrentIndex = navigator.History.Count;

				model.Items = new List<ListItem>()
				{
					new ListItem("Navigator (snapshot)", navigator),
					//new ListItem("Recursive Tab", new TabSample()),
				};

				model.Notes = "The Navigator class creates a bookmark for every tab change you make, and allows you to move backwards and forwards. The Back/Forward buttons currently use this. Eventually a list/drop down could be used to select the bookmark";
			}
		}
	}
}
