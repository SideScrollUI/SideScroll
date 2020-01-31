using System;
using System.Collections.Generic;
using System.Threading;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridDictionary : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private Dictionary<string, TestItem> items;

			public override void Load(Call call)
			{
				items = new Dictionary<string, TestItem>();
				AddEntries(null);
				tabModel.AddData(items);

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Entries", AddEntries),
				};
			}

			private void AddEntries(Call call)
			{
				for (int i = 0; i < 20; i++)
				{
					TestItem testItem = new TestItem();
					testItem.Name = i.ToString();
					testItem.Value += i * 100;
					items.Add(testItem.Name, testItem);
				}
			}
		}

		public class TestItem
		{
			public string Name { get; set; }
			public int Value { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}
	}
}
