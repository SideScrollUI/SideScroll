using SideScroll.Attributes;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples.Loading;

public class TabSampleLoadAsyncItem : ITab
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

		public class TestItem
		{
			public int Integer { get; set; }

			[Item]
			public async Task<string> Text(Call call)
			{
				await Task.Delay(2000);
				return "Text";
			}
		}
	}
}
