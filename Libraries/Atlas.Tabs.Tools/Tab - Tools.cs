using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Tools
{
	public class TabTools : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("File Browser", new TabFileBrowser()),
				};
			}
		}
	}
}
