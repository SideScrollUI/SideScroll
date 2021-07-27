using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestSkip: ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				var sampleItems = new ItemCollection<SampleItem>()
				{
					new SampleItem(1, "Item 1"),
				};

				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample Items", sampleItems),
				};
			}
		}
	}
}
/*
Should we just collapse instead?
	Performance reasons?
*/
