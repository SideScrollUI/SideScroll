namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleGridHashSet : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private HashSet<TabSampleGridCollectionSize.TestItem> _items = [];

		public override void Load(Call call, TabModel model)
		{
			_items = [];
			AddEntries();
			model.AddData(_items);

			// HashSet not observable
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
				var testItem = new TabSampleGridCollectionSize.TestItem
				{
					SmallNumber = index,
				};
				testItem.BigNumber += index;
				_items.Add(testItem);
			}
		}
	}
}
