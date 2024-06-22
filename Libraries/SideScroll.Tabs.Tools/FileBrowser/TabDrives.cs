using SideScroll;

namespace SideScroll.Tabs.Tools;

public class TabDrives(FileSelectorOptions? fileSelectorOptions = null) : ITab
{
	public FileSelectorOptions? FileSelectorOptions = fileSelectorOptions;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabDrives tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			DriveInfo[] drives = DriveInfo.GetDrives();

			model.Items = drives
				.Select(d => new TabDirectory(d.Name, tab.FileSelectorOptions))
				.ToList();
		}
	}
}
