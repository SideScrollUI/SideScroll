using Atlas.Core;
using System;

namespace Atlas.Tabs.Test.Exceptions
{
	public class TabTestLoadException : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				throw new Exception("Load exception");
			}
		}
	}
}
