using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestMemory : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private byte[] bytes;

			public override void Load()
			{
				//bytes = new byte[500000000]; // 500 MB, creates 200k strings using ListToString
				bytes = new byte[128];
				Array.Clear(bytes, 0, bytes.Length); // toggle the memory so it gets used
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Bytes", bytes),
				};
			}
		}
	}
}
/*
For testing memory usage and leaks?
*/
