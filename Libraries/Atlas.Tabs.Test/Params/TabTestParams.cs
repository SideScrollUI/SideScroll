using Atlas.Core;

namespace Atlas.Tabs.Test;

public class TabTestParams : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new ItemCollection<ListItem>()
			{
				new("Tasks", new TabTestParamsTasks()),
				new("Collection", new TabTestParamsCollection()),
				new("Data Tabs", new TabTestParamsDataTabs()),
			};
		}
	}
}
