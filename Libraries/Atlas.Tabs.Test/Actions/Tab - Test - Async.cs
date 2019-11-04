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

			public async Task LoadAsync(Call call)
			{
				await Task.Delay(500);
				tabModel.AddObject("Finished");
			}

			public override void Load(Call call)
			{
				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Reload", ReloadInstance),
				};
			}

			private void ReloadInstance(Call call)
			{
				base.Reload();
			}
		}
	}
}

