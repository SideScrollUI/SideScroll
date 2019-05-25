using System;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestWideColumns : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<TestWideItem> items;

			public override void Load(Call call)
			{
				items = new ItemCollection<TestWideItem>();
				AddEntries();
				tabModel.Items = items;

				/*ItemCollection<TaskCreator> actions = new ItemCollection<TaskCreator>();
				//actions.Add(new TaskAction("Add Entries", AddEntries));
				tabModel.Actions = actions;*/
			}

			private void AddEntries()
			{
				for (int i = 0; i < 20; i++)
				{
					TestWideItem testItem = new TestWideItem();
					testItem.smallNumber = i;
					testItem.bigNumber += i;
					items.Add(testItem);
				}
			}
		}

		public class TestWideItem
		{
			public int smallNumber { get; set; } = 0;
			public long bigNumber { get; set; } = 1234567890123456789;
			public string longText0 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
			[ColumnMaxWidth(200)]
			public string longText1 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
			public string longText2 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
			public string longText3 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
			public string longText4 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
			public string longText5 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
			public string longText6 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
			public string longText7 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
			public string longText8 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";
			public string longText9 { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";

			public override string ToString()
			{
				return smallNumber.ToString();
			}
		}
	}
}
