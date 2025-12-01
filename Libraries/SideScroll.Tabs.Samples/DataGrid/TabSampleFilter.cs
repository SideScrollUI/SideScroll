namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleFilter : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			string characters = "abcdefghijklmn";
			List<TestFilterItem> items = [];
			for (int i = 0; i < 10; i++)
			{
				var item = new TestFilterItem("Item " + i, i)
				{
					Child = new TestFilterItem(characters[i].ToString(), i)
				};

				items.Add(item);
			}

			model.Items = items;
			model.ShowSearch = true;
		}
	}

	public record TestFilterItem(string Text, int Number)
	{
		//[InnerValue]
		public TestFilterItem? Child;

		public override string ToString() => Text;
	}
}
