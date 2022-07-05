using Atlas.Core;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Test.DataGrid;

public class TabTestGridCollectionSize : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private ItemCollection<TestItem>? _items;

		public override void Load(Call call, TabModel model)
		{
			_items = new ItemCollection<TestItem>();
			AddEntries(20);
			model.Items = _items;

			model.Actions = new List<TaskCreator>()
			{
				new TaskAction("Add 100 Entries", () => AddEntries(100)),
				new TaskAction("Add 1,000 Entries", () => AddEntries(1000)),
				new TaskAction("Add 10,000 Entries", () => AddEntries(10000)),
				new TaskAction("Add 100,000 Entries (WPF Only)", () => AddEntries(100000)),
				new TaskAction("Add 1,000,000 Entries (WPF Only)", () => AddEntries(1000000)),
			};
			//actions.Add(new TaskAction("Add Entries", AddEntries));
		}

		private void AddEntries(int count)
		{
			for (int i = 0; i < count; i++)
			{
				int number = _items!.Count;
				var testItem = new TestItem
				{
					SmallNumber = number
				};
				testItem.BigNumber += number;
				if (number > 0)
					testItem.Size = number * 100;
				_items.Add(testItem);
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
		public string LongText { get; set; } = "abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij abcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghijklmnopqrztuvwxyzabcdefghij";

		public Uri Uri { get; set; } = new Uri("http://localhost");
		public int? Size { get; set; }

		public override string ToString() => SmallNumber.ToString();
	}
}
