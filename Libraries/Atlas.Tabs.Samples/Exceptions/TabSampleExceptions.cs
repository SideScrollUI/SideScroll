using Atlas.Core;

namespace Atlas.Tabs.Samples.Exceptions;

public class TabSampleExceptions : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new ItemCollection<ListItem>
			{
				new("Load Exception", new TabSampleLoadException()),
			};

			call.Log.AddError("Load error");
		}
	}
}
