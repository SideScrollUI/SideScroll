using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid
{
	public class TabTestNullableArray : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				TestItem?[] testItems = new TestItem?[10];

				for (int i = 0; i < testItems.Length; i++)
				{
					testItems[i] = new TestItem()
					{
						smallNumber = i,
						bigNumber = i * 1000,
					};
				}
				//tabModel.Items = testItems;
				tabModel.AddData(testItems);
			}
		}

		public struct TestItem
		{
			public int smallNumber { get; set; }
			public long bigNumber { get; set; }

			public override string ToString()
			{
				return smallNumber.ToString();
			}
		}
	}
}
