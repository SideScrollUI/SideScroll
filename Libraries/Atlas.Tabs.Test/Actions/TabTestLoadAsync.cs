using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atlas.Tabs.Test.Actions;

public class TabTestLoadAsync : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance, ITabAsync
	{
		private const int DelayMs = 2000;

		public async Task LoadAsync(Call call, TabModel model)
		{
			call.Log.Add("Sleeping", new Tag("Milliseconds", DelayMs));

			await Task.Delay(DelayMs);

			model.AddObject("Finished");

			model.Items = new List<int>()
			{
				1, 2, 3
			};

			model.Actions = new List<TaskCreator>()
			{
				new TaskDelegate("Reload", ReloadInstance),
			};
		}

		private void ReloadInstance(Call call)
		{
			Reload();
		}
	}
}

