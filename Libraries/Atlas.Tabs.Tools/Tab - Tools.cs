using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Tools
{
	public class TabTools : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load()
			{
				ItemCollection<ListItem> items = new ItemCollection<ListItem>();
				items.Add(new ListItem("File Browser", new TabFileBrowser()));
				tabModel.Items = items;
			}
		}
	}
}
