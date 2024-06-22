namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleFilter : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			string characters = "abcdefghijklmn";
			var items = new List<TestFilterItem>();
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

			model.Notes = @"
* Press Ctrl-F on any Data Grid to add a filter (You can click anywhere on a tab to focus it)
* You can use | or & to restrict searches
* Examples:
  - Search for the exact string ""ABC"" or anything containing 123
	- ""ABC"" | 123
* Recursive searches will eventually be supported
";
		}
	}

	public record TestFilterItem(string Text, int Number)
	{
		//[InnerValue]
		public TestFilterItem? Child;

		public override string ToString() => Text;
	}
}
