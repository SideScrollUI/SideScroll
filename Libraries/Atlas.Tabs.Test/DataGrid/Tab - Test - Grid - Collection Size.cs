using System;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridCollectionSize : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<TestItem> items;

			public override void Load(Call call)
			{
				items = new ItemCollection<TestItem>();
				AddEntries(20);
				tabModel.Items = items;

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskAction("Add 100 Entries", new Action(() => AddEntries(100))),
					new TaskAction("Add 1,000 Entries", new Action(() => AddEntries(1000))),
					new TaskAction("Add 10,000 Entries", new Action(() => AddEntries(10000))),
					new TaskAction("Add 100,000 Entries (WPF Only)", new Action(() => AddEntries(100000))),
					new TaskAction("Add 1,000,000 Entries (WPF Only)", new Action(() => AddEntries(1000000))),
				};
				//actions.Add(new TaskAction("Add Entries", AddEntries));
			}

			private void AddEntries(int count)
			{
				for (int i = 0; i < count; i++)
				{
					int number = items.Count;
					TestItem testItem = new TestItem();
					testItem.smallNumber = number;
					testItem.bigNumber += number;
					if (number > 0)
						testItem.Size = number * 100;
					items.Add(testItem);
				}
			}
		}

		public class TestItem
		{
			[ButtonColumn("-")]
			public void Click()
			{

			}

			public int smallNumber { get; set; } = 0;
			public long bigNumber { get; set; } = 1234567890123456789;
			public string longText0 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";

			public Uri Uri { get; set; } = new Uri("http://localhost");
			public int? Size { get; set; }
			//public string longText { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyz";

			public override string ToString()
			{
				return smallNumber.ToString();
			}
		}
	}
}
