﻿using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Tools
{
	public class TabFileBrowser : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				ItemCollection<ListItem> items = new ItemCollection<ListItem>();
				items.Add(new ListItem("Data", new TabDirectory(project.userSettings.ProjectPath)));
				model.Items = items;
			}
		}
	}
}
