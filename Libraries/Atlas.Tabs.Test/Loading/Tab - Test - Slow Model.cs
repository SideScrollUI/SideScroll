using Atlas.Core;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Test
{
	public class TabTestSlowModel: ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Test Item", new TestItem()),
				};
			}
		}

		public class TestItem
		{
			public int Integer { get; set; }

			private string _Text;
			public string Text
			{
				get
				{
					if (_Text == null)
					{
						System.Threading.Thread.Sleep(5000);
						_Text = "Text";
					}
					return _Text;
				}
			}
		}
	}
}
