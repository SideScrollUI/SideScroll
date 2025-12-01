namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleGridDictionary : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
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
				TestItem testItem = new()
				{
					Name = index.ToString(),
					Value = index * 100,
				};
				_items.Add(testItem.Name, testItem);
			}
		}
	}

	private class TestItem
	{
		public string? Name { get; set; }
		public int Value { get; set; }

		public override string? ToString() => Name;
	}
}
