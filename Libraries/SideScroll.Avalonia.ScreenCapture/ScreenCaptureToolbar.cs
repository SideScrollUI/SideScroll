using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Resources;
using SideScroll.Utilities;
using System.Runtime.InteropServices;

namespace SideScroll.Avalonia.ScreenCapture;

public class ScreenCaptureToolbar : TabControlToolbar
{
	protected override Type StyleKeyOverride => typeof(TabControlToolbar);

	public TabViewer TabViewer { get; init; }

	public ToolbarButton? ButtonCopyClipboard { get; set; }
	public ToolbarButton ButtonSave { get; set; }
	public ToolbarButton ButtonClose { get; set; }

	public ScreenCaptureToolbar(TabViewer tabViewer) : base(null)
	{
		TabViewer = tabViewer;

		OSPlatform platform = ProcessUtils.GetOSPlatform();
		if (platform != OSPlatform.Linux)
		{
			ButtonCopyClipboard = AddButton("Copy to Clipboard", Icons.Svg.PadNote);
		}
		ButtonSave = AddButton("Save", Icons.Svg.Save);

		AddSeparator();
		ButtonClose = AddButton("Close Snapshot", Icons.Svg.Delete);
	}
}
