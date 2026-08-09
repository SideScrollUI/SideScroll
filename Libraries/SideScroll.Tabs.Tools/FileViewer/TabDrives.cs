using SideScroll.Attributes;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tools.FileViewer;

[PrivateData]
public class TabDrives(FileSelectorOptions? fileSelectorOptions = null) : ITab
{
	public FileSelectorOptions? FileSelectorOptions { get; set; } = fileSelectorOptions;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabDrives tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			List<ListPair> items = [];
			foreach (DriveInfo drive in drives)
			{
				try
				{
					string status = drive.IsReady ? drive.VolumeLabel : "Not Ready";
					items.Add(new ListPair(drive.Name, status, new TabDirectory(drive.Name, tab.FileSelectorOptions)));
				}
				catch (Exception e)
				{
					call.Log.Add(e, new Tag("Drive", drive.Name));
					items.Add(new ListPair(drive.Name, "Unavailable"));
				}
			}

			model.Items = items;
		}
	}
}
