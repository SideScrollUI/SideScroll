using SideScroll.Collections;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleHeaders : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Show Headers", BuildShowHeaders()),
				new("Hide Headers", BuildHideHeaders()),
			};
		}

		private static ItemCollection<string> BuildShowHeaders()
		{
			ItemCollection<string> items =
			[
				"Item 1",
				"Item 2",
				"Item 3",
			];
			items.ShowHeader = true;
			return items;
		}

		private static ItemCollection<TestItem> BuildHideHeaders()
		{
			ItemCollection<TestItem> items = [];
			for (int i = 0; i < 10; i++)
			{
				items.Add(new TestItem($"Test {i}", i));
			}
			items.ShowHeader = false;
			return items;
		}

		private class TestItem(string id, int value)
		{
			public string Id => id;
			public int Value => value;
		}
	}
}
