using SideScroll.Tasks;
using SideScroll.Utilities;

namespace SideScroll.Tabs.Samples;

public class TabSampleProcess : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Actions =
			[
				new TaskDelegate("Open Folder", OpenFolder, true),
				new TaskDelegate("Open Browser", OpenBrowser, true),
			];
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
