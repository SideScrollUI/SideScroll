using Atlas.Core;
using Atlas.Resources;

namespace Atlas.Tabs.Samples.Objects;

public class TabSampleJson : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Sample Text", LazyJsonNode.Parse(TextSamples.Json)),
			};
		}
	}
}
