using Atlas.Core.Utilities;
using Atlas.Resources;
using Atlas.UI.Avalonia.Controls.Toolbar;
using Atlas.UI.Avalonia.Viewer;
using System.Runtime.InteropServices;

namespace Atlas.UI.Avalonia.ScreenCapture;

public class ScreenCaptureToolbar : TabControlToolbar
{
	public TabViewer TabViewer;

	public ToolbarButton? ButtonCopyClipboard;
	public ToolbarButton ButtonSave;
	//public ToolbarButton ButtonLink;
	public ToolbarButton ButtonClose;

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
