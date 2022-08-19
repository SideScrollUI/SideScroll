using Atlas.Core;

namespace Atlas.Tabs.Test;

public class TabTestBrowser : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>()
			{
				new("Uri", new Uri("https://wikipedia.org")),
			};

			model.Actions = new List<TaskCreator>()
			{
				new TaskDelegate("Open Browser", OpenBrowser),
			};
		}

		private void OpenBrowser(Call call)
		{
			ProcessUtils.OpenBrowser("http://wikipedia.org");
		}
	}
}
