using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestLoadException : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				throw new Exception("Load exception");
			}
		}
	}
}
