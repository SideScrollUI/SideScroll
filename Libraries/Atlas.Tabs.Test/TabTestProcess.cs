using Atlas.Core;

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

		private void OpenFolder(Call call)
		{
			ProcessUtils.OpenFolder(Directory.GetCurrentDirectory());
		}
	}
}
