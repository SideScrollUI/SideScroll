using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleBytes : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("100 Bytes", Create(100)),
				new("1,000 Bytes", Create(1_000)),
				new("10,000 Bytes", Create(10_000)),
				new("100,000 Bytes", Create(100_000)),
				new("1,000,000 Bytes", Create(1_000_000)),
			};
		}

		private byte[] Create(int size)
		{
			var bytes = new byte[size];
			for (int i = 0; i < size; i++)
			{
				bytes[i] = (byte)i;
			}
			return bytes;
		}
	}
}
