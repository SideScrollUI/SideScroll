using Atlas.Core;
using Atlas.Resources;

namespace Atlas.Tabs.Test.Objects;

public class TabTestJson : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Sample Text", LazyJsonNode.Parse(Samples.Text.Json)),
			};
		}
	}
}
