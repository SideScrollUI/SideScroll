using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid;

public class TabTestFilter : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			string characters = "abcdefghijklmn";
			var items = new ItemCollection<TestFilterItem>();
			for (int i = 0; i < 2; i++)
			{
				var item = new TestFilterItem()
				{
					Text = "Item " + i,
					Number = i
				};

				var child = new TestFilterItem()
				{
					Text = characters[i].ToString(),
					Number = i
				};
				item.Child = child;

				items.Add(item);
			}

			model.Items = items;

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

	public class TestFilterItem
	{
		public string? Text { get; set; }
		public int Number { get; set; }
		//[InnerValue]
		public TestFilterItem? Child;

		public override string? ToString() => Text;
	}
}
