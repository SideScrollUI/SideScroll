using Atlas.Core;
using System.Collections.Generic;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestGridHashSet : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private HashSet<TabTestGridCollectionSize.TestItem> items;

			public override void Load(Call call, TabModel model)
			{
				items = new HashSet<TabTestGridCollectionSize.TestItem>();
				AddEntries(null);
				model.AddData(items);

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add Entries", AddEntries),
				};
			}

			private void AddEntries(Call call)
			{
				for (int i = 0; i < 20; i++)
				{
					var testItem = new TabTestGridCollectionSize.TestItem();
					testItem.SmallNumber = i;
					testItem.BigNumber += i;
					items.Add(testItem);
				}
			}
		}
	}
}
