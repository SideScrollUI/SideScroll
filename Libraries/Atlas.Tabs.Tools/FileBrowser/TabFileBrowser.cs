using Atlas.Core;
using System.IO;

namespace Atlas.Tabs.Tools;

[ListItem]
public class TabFileBrowser
{
	public TabDirectory Current => new(Directory.GetCurrentDirectory());
	public TabDrives Drives => new();
}
