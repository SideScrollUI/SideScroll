using System;
using System.Collections.Generic;
using System.Text;
using Atlas.Core;

namespace Atlas.Tabs.Test.Chart
{
	public class TabTestChart : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load()
			{
				tabModel.Notes = "";
				tabModel.Items = new ItemCollection<ListItem>()
				{
					//new ListItem("Split", new TabTestChartSplit()),
					new ListItem("Overlay", new TabTestChartOverlay()),
				};
			}
		}
	}
}
/*

*/
