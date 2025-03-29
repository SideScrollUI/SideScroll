using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tools.FileViewer;

public class TabDrives(FileSelectorOptions? fileSelectorOptions = null) : ITab
{
	public FileSelectorOptions? FileSelectorOptions { get; set; } = fileSelectorOptions;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabDrives tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			DriveInfo[] drives = DriveInfo.GetDrives();

			model.Items = drives
				.Select(drive => new ListPair(drive.Name, drive.VolumeLabel, new TabDirectory(drive.Name, tab.FileSelectorOptions)))
				.ToList();
		}
	}
}
