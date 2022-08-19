using Atlas.Core;

namespace Atlas.Tabs.Tools;

public class TabDrives : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			DriveInfo[] drives = DriveInfo.GetDrives();

			model.Items = drives
				.Select(d => new TabDirectory(d.Name))
				.ToList();
		}
	}
}
