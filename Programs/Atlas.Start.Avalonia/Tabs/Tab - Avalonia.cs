using System;
using Atlas.Core;
using Atlas.Tabs;
using Atlas.Tabs.Test;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabAvalonia : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Test", new TabTest()),
					new ListItem("Custom Control", new TabCustomControl()),
					new ListItem("Icons", new TabIcons()),
					//new ListItem("Demo", new TabDemo()),
					//new ListItem("SeriLog", new TabSeriLog()),
					//new ListItem("Inputs", new TabParams()),
				};
			}
		}
	}
}
/*

*/
