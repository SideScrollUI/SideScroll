using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Tabs.Tools;

public class TabFileBrowser : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>()
			{
				new("Current", new TabDirectory(Directory.GetCurrentDirectory())),
				new("Root", new TabDirectory("")),
			};
		}
	}
}
