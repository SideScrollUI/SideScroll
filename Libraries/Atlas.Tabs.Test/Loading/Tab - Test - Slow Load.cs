﻿using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestSlowLoad : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				System.Threading.Thread.Sleep(5000);
				model.AddObject("Finished");
			}
		}
	}
}
