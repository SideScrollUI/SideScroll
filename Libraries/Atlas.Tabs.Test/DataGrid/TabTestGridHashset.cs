using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid;

public class TabTestGridHashSet : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private HashSet<TabTestGridCollectionSize.TestItem>? _items;

		public override void Load(Call call, TabModel model)
		{
			_items = new HashSet<TabTestGridCollectionSize.TestItem>();
			AddEntries(null);
			model.AddData(_items);

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add Entries", AddEntries),
			};
		}

		private void AddEntries(Call? call)
		{
			for (int i = 0; i < 20; i++)
			{
				var testItem = new TabTestGridCollectionSize.TestItem
				{
					SmallNumber = i
				};
				testItem.BigNumber += i;
				_items!.Add(testItem);
			}
		}
	}
}
