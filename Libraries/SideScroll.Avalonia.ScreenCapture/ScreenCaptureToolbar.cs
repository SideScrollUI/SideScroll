using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Resources;
using SideScroll.Utilities;
using System.Runtime.InteropServices;

namespace SideScroll.Avalonia.ScreenCapture;

public class ScreenCaptureToolbar : TabControlToolbar
{
	protected override Type StyleKeyOverride => typeof(TabControlToolbar);

	public ToolbarButton? ButtonCopyClipboard { get; set; }
	public ToolbarButton ButtonSave { get; set; }
	public ToolbarButton ButtonOpenFolder { get; set; }
	public ToolbarButton ButtonClose { get; set; }

	public ScreenCaptureToolbar() : base(null)
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
