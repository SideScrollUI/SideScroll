using Atlas.Core;

namespace Atlas.Tabs.Test.Loading;

public class TabTestLoading : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new ItemCollection<ListItem>()
			{
				new("Slow Load", new TabTestSlowLoad()),
				new("Slow Model", new TabTestSlowModel()),
				new("Slow Async Item", new TabTestSlowAsyncItem()),
				new("Slow Async Model", new TabTestSlowAsyncModel()),
			};
		}
	}
}
