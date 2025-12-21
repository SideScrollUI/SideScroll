using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Resources;
using SideScroll.Utilities;
using System.Runtime.InteropServices;

namespace SideScroll.Avalonia.Controls.ScreenCapture;

public class ScreenCaptureToolbar : TabControlToolbar
{
	protected override Type StyleKeyOverride => typeof(TabControlToolbar);

	public ToolbarButton? ButtonCopyClipboard { get; }
	public ToolbarButton ButtonSave { get; }
	public ToolbarButton ButtonOpenFolder { get; }
	public ToolbarButton ButtonClose { get; }

	public ScreenCaptureToolbar()
	{
		OSPlatform platform = ProcessUtils.GetOSPlatform();
		if (platform != OSPlatform.Linux)
		{
			ButtonCopyClipboard = AddButton("Copy to Clipboard", Icons.Svg.Copy);
		}
		ButtonSave = AddButton("Save", Icons.Svg.Save);

		AddSeparator();
		ButtonOpenFolder = AddButton("Open Folder", Icons.Svg.OpenFolder);

		AddSeparator();
		ButtonClose = AddButton("Close Snapshot", Icons.Svg.Delete);
	}
}
