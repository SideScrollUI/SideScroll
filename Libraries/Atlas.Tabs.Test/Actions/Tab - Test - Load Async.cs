using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.Tabs.Test.Actions
{
	public class TabTestLoadAsync : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance, ITabAsync
		{
			private const int delayMs = 5000;

			public async Task LoadAsync(Call call)
			{
				call.log.Add("Sleeping", new Tag("Milliseconds", delayMs));
				await Task.Delay(delayMs);
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

