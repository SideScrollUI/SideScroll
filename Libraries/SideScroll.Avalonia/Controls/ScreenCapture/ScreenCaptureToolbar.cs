using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Resources;
using SideScroll.Utilities;
using System.Runtime.InteropServices;

namespace SideScroll.Avalonia.Controls.ScreenCapture;

/// <summary>
/// The toolbar for the screen capture control, providing buttons to copy, save, open the save folder, and close.
/// </summary>
public class ScreenCaptureToolbar : TabControlToolbar
{
	/// <inheritdoc/>
	protected override Type StyleKeyOverride => typeof(TabControlToolbar);

	/// <summary>Gets the copy-to-clipboard button, or <c>null</c> on Linux where clipboard bitmap support is unavailable.</summary>
	public ToolbarButton? ButtonCopyToClipboard { get; }

	/// <summary>Gets the save-to-file button.</summary>
	public ToolbarButton ButtonSave { get; }

	/// <summary>Gets the open-folder button.</summary>
	public ToolbarButton ButtonOpenFolder { get; }

	/// <summary>Gets the close snapshot button.</summary>
	public ToolbarButton ButtonClose { get; }

	/// <summary>Initializes a new <see cref="ScreenCaptureToolbar"/>, adding platform-appropriate copy, save, open-folder, and close buttons.</summary>
	public ScreenCaptureToolbar()
	{
		OSPlatform platform = ProcessUtils.GetOSPlatform();
		if (platform != OSPlatform.Linux)
		{
			ButtonCopyToClipboard = AddButton("Copy to Clipboard", Icons.Svg.Copy);
		}
		ButtonSave = AddButton("Save", Icons.Svg.Save);

		AddSeparator();
		ButtonOpenFolder = AddButton("Open Folder", Icons.Svg.OpenFolder);

		AddSeparator();
		ButtonClose = AddButton("Close Snapshot", Icons.Svg.Close);
	}
}
