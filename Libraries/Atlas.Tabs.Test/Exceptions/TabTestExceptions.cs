using Atlas.Core;

namespace Atlas.Tabs.Test.Exceptions
{
	public class TabTestExceptions : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Load Exception", new TabTestLoadException()),
				};

				call.Log.AddError("Load error");
			}
		}
	}
}
