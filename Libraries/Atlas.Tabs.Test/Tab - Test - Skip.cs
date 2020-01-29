using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestSkip: ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				// Replace this
				var sampleItems = new ItemCollection<SampleItem>();
				sampleItems.Add(new SampleItem(sampleItems.Count, "Item " + sampleItems.Count.ToString()));

				tabModel.AddData("This should be skipped");

				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample Items", sampleItems),
				};
			}
		}
	}
}
/*
Should we just collapse instead?
	Performance reasons?
*/
