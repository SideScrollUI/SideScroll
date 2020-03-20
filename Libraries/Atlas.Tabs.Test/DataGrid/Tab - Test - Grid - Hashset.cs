using System;
using System.Collections.Generic;
using System.Threading;
using Atlas.Core;

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

				ItemCollection<TaskCreator> actions = new ItemCollection<TaskCreator>();
				actions.Add(new TaskDelegate("Add Entries", AddEntries));
				model.Actions =  actions;
			}

			private void AddEntries(Call call)
			{
				for (int i = 0; i < 20; i++)
				{
					TabTestGridCollectionSize.TestItem testItem = new TabTestGridCollectionSize.TestItem();
					testItem.smallNumber = i;
					testItem.bigNumber += i;
					items.Add(testItem);
				}
			}
		}
	}
}
