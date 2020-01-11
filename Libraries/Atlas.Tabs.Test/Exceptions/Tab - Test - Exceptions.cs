using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestExceptions : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Load Exception", new TabTestLoadException()),
				};

				call.log.AddError("Load error");
			}
		}
	}
}
