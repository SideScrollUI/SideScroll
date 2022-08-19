using Atlas.Core;

namespace Atlas.Tabs.Test.Loading;

public class TabTestSlowAsyncModel : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new ItemCollection<ListItem>()
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
				await Task.Delay(1000);
				return "Text";
			}
		}
	}
}
