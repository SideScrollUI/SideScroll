using Atlas.Core;
using System;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridCollectionSize : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private ItemCollection<TestItem> items;

			public override void Load(Call call, TabModel model)
			{
				items = new ItemCollection<TestItem>();
				AddEntries(20);
				model.Items = items;

				model.Actions =  new ItemCollection<TaskCreator>()
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
					var testItem = new TestItem();
					testItem.SmallNumber = number;
					testItem.BigNumber += number;
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

			public int SmallNumber { get; set; } = 0;
			public long BigNumber { get; set; } = 1234567890123456789;
			public string LongText0 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";

			public Uri Uri { get; set; } = new Uri("http://localhost");
			public int? Size { get; set; }
			//public string longText { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyz";

			public override string ToString()
			{
				return SmallNumber.ToString();
			}
		}
	}
}
