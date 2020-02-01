using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestSlowModel: ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Test Item", new TestItem()),
				};
			}
		}

		public class TestItem
		{
			public int Integer { get; set; }

			public string Text
			{
				get
				{
					System.Threading.Thread.Sleep(5000);
					return "Text";
				}
			}
		}
	}
}
