using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestBrowser : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load()
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Uri", new Uri("https://wikipedia.org")),
				};

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Open Browser", OpenBrowser),
				};
			}

			private void OpenBrowser(Call call)
			{
				ProcessUtils.OpenBrowser("http://wikipedia.org");
			}
		}
	}
}
