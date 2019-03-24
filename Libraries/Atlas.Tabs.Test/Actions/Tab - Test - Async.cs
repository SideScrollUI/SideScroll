using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.Tabs.Test.Actions
{
	public class TabTestAsync : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance, ITabAsync
		{
			//private ItemCollection<ListItem> items;

			public async Task LoadAsync()
			{
				await Task.Delay(2000);
				tabModel.AddObject("Finished");
			}

		}
	}
}

