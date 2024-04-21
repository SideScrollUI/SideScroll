using Atlas.Core;
using Atlas.Core.Tasks;
using Atlas.Core.Utilities;

namespace Atlas.Tabs.Test;

public class TabTestProcess : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Open Folder", OpenFolder, true),
				new TaskDelegate("Open Browser", OpenBrowser, true),
			};
		}

		private static void OpenFolder(Call call)
		{
			ProcessUtils.OpenFolder(Directory.GetCurrentDirectory());
		}

		private static void OpenBrowser(Call call)
		{
			ProcessUtils.OpenBrowser("https://wikipedia.org");
		}
	}
}
