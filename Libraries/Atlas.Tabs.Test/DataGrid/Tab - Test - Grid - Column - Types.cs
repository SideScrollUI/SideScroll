using System;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridColumnTypes : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load()
			{
				ItemCollection<TestItem> items = new ItemCollection<TestItem>();
				for (int i = 0; i < 10; i++)
				{
					TestItem testItem = new TestItem()
					{
						Integer = i,
						Long = (long)i * (long)int.MaxValue,
						DateTime = new DateTime(DateTime.Now.Ticks + i),
						Bool = (i % 2 == 1),
					};
					testItem.LongString = testItem.LongString + i.ToString(); // make as a unique string
					items.Add(testItem);
				}
				tabModel.Items = items;
			}
		}

		public class TestItem
		{
			public int Integer { get; set; } = 0;
			public long Long { get; set; } = 1234567890123456789;
			public bool Bool { get; set; }
			public byte[] ByteArray { get; set; } = new byte[256];
			public DateTime DateTime { get; set; }
			public string SmallString { get; set; } = "Text";
			public string LongString { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
		}
	}
}
