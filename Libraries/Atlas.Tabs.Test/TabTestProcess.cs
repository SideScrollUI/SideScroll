using Atlas.Core;
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
			};
		}

		private static void OpenFolder(Call call)
		{
			ProcessUtils.OpenFolder(Directory.GetCurrentDirectory());
		}
	}
}
