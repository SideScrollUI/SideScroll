namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleGridColumnTypes : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var items = new List<ManyTypesItem>();
			for (int i = 0; i < 10; i++)
			{
				var testItem = new ManyTypesItem
				{
					Integer = i,
					Long = (long)i * int.MaxValue,
					Double = i / 3.0,
					DateTime = DateTime.Now.AddHours(i),
					TimeSpan = TimeSpan.FromHours(i),
					Bool = (i % 2 == 1),
				};

				if (i % 2 == 0)
				{
					testItem.Object = (i % 4 == 0);
				}

				for (int j = 0; j < i; j++)
				{
					testItem.IntegerList.Add(j);
				}

				testItem.LongString += i; // make as a unique string
				items.Add(testItem);
			}
			model.Items = items;
		}
	}

	public class ManyTypesItem
	{
		public int Integer { get; set; } = 123;
		public long Long { get; set; } = 1234567890123456789;
		public double Double { get; set; } = 123.456;
		public decimal Decimal { get; set; } = 123456789.0123456789M;
		public bool Bool { get; set; }
		public byte[] ByteArray { get; set; } = new byte[256];
		public List<int> IntegerList { get; set; } = [];
		public DateTime DateTime { get; set; }
		public TimeSpan TimeSpan { get; set; }
		public object? Object { get; set; }
		public string ShortString { get; set; } = "Text";
		public string LongString { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";

		public override string ToString() => Integer.ToString();
	}
}
