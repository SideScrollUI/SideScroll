using SideScroll.Attributes;

namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleGridColumnSizing : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			List<ManyTypesItem> items = [];
			for (int i = 0; i < 10; i++)
			{
				ManyTypesItem testItem = new()
				{
					Integer = i,
					Long = (long)i * int.MaxValue,
					DateTime = DateTime.Now.AddHours(i),
					TimeSpan = TimeSpan.FromHours(i),
					Bool = i % 2 == 1,
				};

				testItem.LongString += i; // make as a unique string
				items.Add(testItem);
			}
			model.Items = items;
		}
	}

	private class ManyTypesItem
	{
		public int Integer { get; set; }
		public long Long { get; set; } = 1234567890123456789;
		public bool Bool { get; set; }

		public DateTime DateTime { get; set; }

		[Formatted]
		public TimeSpan TimeSpan { get; set; }

		[MaxWidth(200)]
		public string ShortString { get; set; } = "Text";

		[MaxWidth(200)]
		public string ShowPrefix { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";

		[MaxWidth(1000), WordWrap]
		public string LongString { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";

		public override string ToString() => Integer.ToString();
	}
}
