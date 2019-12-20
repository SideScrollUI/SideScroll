using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Core;

namespace Atlas.Tabs.Test.Actions
{
	public class TabTestLoadAsync : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance, ITabAsync
		{
			private const int delayMs = 2000;

			public async Task LoadAsync(Call call)
			{
				call.log.Add("Sleeping", new Tag("Milliseconds", delayMs));
				await Task.Delay(delayMs);
				tabModel.AddObject("Finished");

				tabModel.Items = new List<int>()
				{
					1, 2, 3
				};
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

