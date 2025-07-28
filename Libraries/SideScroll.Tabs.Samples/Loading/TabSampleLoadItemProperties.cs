using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples.Loading;

public class TabSampleLoadItemProperties : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Test Item", new TestItem()),
			};
		}
	}

	public class TestItem
	{
		public int Integer { get; set; }

		private string? _text;
		public string Text
		{
			get
			{
				if (_text == null)
				{
					Thread.Sleep(5000);
					_text = "Text";
				}
				return _text;
			}
		}
	}
}
