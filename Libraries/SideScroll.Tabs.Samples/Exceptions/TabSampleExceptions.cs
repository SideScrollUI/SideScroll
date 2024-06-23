using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples.Exceptions;

public class TabSampleExceptions : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Load Exception", new TabSampleLoadException()),
			};

			call.Log.AddError("Load error");
		}
	}
}
