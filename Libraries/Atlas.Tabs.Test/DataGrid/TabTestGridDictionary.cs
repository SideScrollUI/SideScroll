using Atlas.Core;
using System.Collections.Generic;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridDictionary : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private Dictionary<string, TestItem> _items;

			public override void Load(Call call, TabModel model)
			{
				_items = new Dictionary<string, TestItem>();
				AddEntries(null);
				model.AddData(_items);

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add Entries", AddEntries),
				};
			}

			private void AddEntries(Call call)
			{
				for (int i = 0; i < 20; i++)
				{
					var testItem = new TestItem()
					{
						Name = i.ToString(),
						Value = i * 100,
					};
					_items.Add(testItem.Name, testItem);
				}
			}
		}

		public class TestItem
		{
			public string Name { get; set; }
			public int Value { get; set; }

			public override string ToString() => Name;
		}
	}
}
