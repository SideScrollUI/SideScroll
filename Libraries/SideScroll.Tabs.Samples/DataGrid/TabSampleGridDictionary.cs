namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleGridDictionary : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private Dictionary<string, TestItem> _items = [];

		public override void Load(Call call, TabModel model)
		{
			_items = [];
			AddEntries();
			model.AddData(_items);

			// Dictionary not observable
			/*model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add Entries", AddEntries),
			};*/
		}

		private void AddEntries()
		{
			for (int i = 0; i < 20; i++)
			{
				int index = _items.Count;
				var testItem = new TestItem
				{
					Name = index.ToString(),
					Value = index * 100,
				};
				_items.Add(testItem.Name, testItem);
			}
		}
	}

	public class TestItem
	{
		public string? Name { get; set; }
		public int Value { get; set; }

		public override string? ToString() => Name;
	}
}
