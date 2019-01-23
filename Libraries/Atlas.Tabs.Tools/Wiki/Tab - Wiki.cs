using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Atlas.Tabs.Tools
{
	public class TabWiki : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load()
			{
				ItemCollection<ListItem> items = new ItemCollection<ListItem>();
				//items.Add(new ListItem("Entries", new TabFileBrowser()));
				tabModel.Items = items;
			}
		}
	}
}
/*
*/
