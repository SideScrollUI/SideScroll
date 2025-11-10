namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleNullableArray : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var testItems = new TestItem?[10];

			for (int i = 0; i < testItems.Length; i++) // Should we allow null values too? i += 2
			{
				testItems[i] = new TestItem
				{
					SmallNumber = i,
					BigNumber = i * 1000,
				};
			}
			//model.Items = testItems;
			model.AddData(testItems);
		}
	}

	public struct TestItem
	{
		public int SmallNumber { get; set; }
		public long BigNumber { get; set; }

		public override readonly string ToString() => SmallNumber.ToString();
	}
}
