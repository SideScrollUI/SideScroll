using System;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridColumnCount : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<TestItem> items;

			public override void Load()
			{
				items = new ItemCollection<TestItem>();
				AddEntries(50);
				tabModel.Items = items;

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskAction("Add 100 Entries", new Action(() => AddEntries(100))),
					new TaskAction("Add 1,000 Entries", new Action(() => AddEntries(1000))),
					new TaskAction("Add 10,000 Entries", new Action(() => AddEntries(10000))),
				};
				//actions.Add(new TaskAction("Add Entries", AddEntries));
			}

			private void AddEntries(int count)
			{
				for (int i = 0; i < count; i++)
				{
					int number = items.Count;
					TestItem testItem = new TestItem();
					testItem.Index = number;
					items.Add(testItem);
				}
			}
		}

		public class TestItem
		{
			public int Index { get; set; } = 0;

			public int Num0 { get; set; } = 0;
			public int Num1 { get; set; } = 1;
			public int Num2 { get; set; } = 2;
			public int Num3 { get; set; } = 3;
			public int Num4 { get; set; } = 4;
			public int Num5 { get; set; } = 5;
			public int Num6 { get; set; } = 6;
			public int Num7 { get; set; } = 7;
			public int Num8 { get; set; } = 8;
			public int Num9 { get; set; } = 9;

			public int Num10 { get; set; } = 0;
			public int Num11 { get; set; } = 1;
			public int Num12 { get; set; } = 2;
			public int Num13 { get; set; } = 3;
			public int Num14 { get; set; } = 4;
			public int Num15 { get; set; } = 5;
			public int Num16 { get; set; } = 6;
			public int Num17 { get; set; } = 7;
			public int Num18 { get; set; } = 8;
			public int Num19 { get; set; } = 9;

			public int Num20 { get; set; } = 0;
			public int Num21 { get; set; } = 1;
			public int Num22 { get; set; } = 2;
			public int Num23 { get; set; } = 3;
			public int Num24 { get; set; } = 4;
			public int Num25 { get; set; } = 5;
			public int Num26 { get; set; } = 6;
			public int Num27 { get; set; } = 7;
			public int Num28 { get; set; } = 8;
			public int Num29 { get; set; } = 9;

			public int Num30 { get; set; } = 0;
			public int Num31 { get; set; } = 1;
			public int Num32 { get; set; } = 2;
			public int Num33 { get; set; } = 3;
			public int Num34 { get; set; } = 4;
			public int Num35 { get; set; } = 5;
			public int Num36 { get; set; } = 6;
			public int Num37 { get; set; } = 7;
			public int Num38 { get; set; } = 8;
			public int Num39 { get; set; } = 9;

			public int Num40 { get; set; } = 0;
			public int Num41 { get; set; } = 1;
			public int Num42 { get; set; } = 2;
			public int Num43 { get; set; } = 3;
			public int Num44 { get; set; } = 4;
			public int Num45 { get; set; } = 5;
			public int Num46 { get; set; } = 6;
			public int Num47 { get; set; } = 7;
			public int Num48 { get; set; } = 8;
			public int Num49 { get; set; } = 9;

			public override string ToString()
			{
				return Index.ToString();
			}
		}
	}
}
/*
*/
