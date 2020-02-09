﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestSlowAsyncItem: ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<IListItem>()
				{
					new ListMethodObject(SlowAsyncItem),
				};
			}

			// todo: fix, this is being called twice and blocking the UI the 1st time
			public async Task<object> SlowAsyncItem(Call call)
			{
				await Task.Delay(1000);
				return "finished";
			}
		}
	}
}