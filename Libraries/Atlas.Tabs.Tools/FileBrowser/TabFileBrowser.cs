using Atlas.Core;

namespace Atlas.Tabs.Tools;

[ListItem]
public class TabFileBrowser
{
	public TabDirectory Current => new(Directory.GetCurrentDirectory());
	public TabDirectory Downloads => new(Paths.DownloadPath);
	public TabDrives Drives => new();
}
