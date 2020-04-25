using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestLoading: ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Slow Load", new TabTestSlowLoad()),
					new ListItem("Slow Model", new TabTestSlowModel()),
					new ListItem("Slow Async Item", new TabTestSlowAsyncItem()),
					new ListItem("Slow Async Model", new TabTestSlowAsyncModel()),
				};
			}
		}
	}
}
