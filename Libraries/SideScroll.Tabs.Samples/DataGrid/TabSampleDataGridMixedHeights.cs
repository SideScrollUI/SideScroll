using SideScroll.Attributes;

namespace SideScroll.Tabs.Samples.DataGrid;

// This tab demonstrates an Avalonia bug when using the mouse wheel to scroll up where the rows jump around
public class TabSampleDataGridMixedHeights : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			List<TestItem> list = [];
			for (int i = 0; i < 100; i++)
			{
				string text = i % 2 == 0 ? "aaa" : "b\nb\nb\nb\nb\nb\nb\nb\nb\nb\nb";
				list.Add(new TestItem(i, text));
			}
			model.Items = list;
		}
	}

	private class TestItem
	{
		public int Index { get; set; } = 0;

		[MaxHeight(300)]
		public string? Text { get; set; }

		public TestItem(int index, string? text)
		{
			Index = index;
			Text = text;
		}

		public override string ToString() => Index.ToString();
	}
}
