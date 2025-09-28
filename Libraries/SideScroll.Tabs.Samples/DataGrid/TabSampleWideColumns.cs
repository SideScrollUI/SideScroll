using SideScroll.Attributes;
using SideScroll.Collections;

namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleWideColumns : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private ItemCollection<TestWideItem> _items = [];

		public override void Load(Call call, TabModel model)
		{
			_items = [];
			AddEntries();
			model.Items = _items;
		}

		private void AddEntries()
		{
			for (int i = 0; i < 100; i++)
			{
				TestWideItem testItem = new()
				{
					SmallNumber = i
				};
				testItem.BigNumber += i;
				if (i % 3 == 0)
				{
					testItem.LongText1 += testItem.LongText0;
				}
				_items.Add(testItem);
			}
		}
	}

	public class TestWideItem
	{
		public int SmallNumber { get; set; } = 123;
		public long BigNumber { get; set; } = 1234567890123456789;

		public string LongText0 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
		[MaxWidth(200), WordWrap]
		public string LongText1 { get; set; } = "abcdefghijklmnopqrz";
		public string LongText2 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
		public string LongText3 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
		public string LongText4 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
		public string LongText5 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
		public string LongText6 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
		public string LongText7 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
		public string LongText8 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
		public string LongText9 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";

		public override string ToString() => SmallNumber.ToString();
	}
}
