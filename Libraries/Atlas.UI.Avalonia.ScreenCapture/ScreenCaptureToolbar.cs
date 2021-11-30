using Atlas.Core;
using Atlas.Resources;
using Atlas.UI.Avalonia.Controls;
using System;
using System.Runtime.InteropServices;

namespace Atlas.UI.Avalonia.ScreenCapture
{
	public class ScreenCaptureToolbar : TabControlToolbar
	{
		public TabViewer TabViewer;

		public ToolbarButton ButtonCopyClipboard;
		public ToolbarButton ButtonSave;
		//public ToolbarButton ButtonLink;
		public ToolbarButton ButtonClose;

		public ScreenCaptureToolbar(TabViewer tabViewer) : base(null)
		{
			TabViewer = tabViewer;
			InitializeControls();
		}

		private void InitializeControls()
		{
			OSPlatform platform = ProcessUtils.GetOSPlatform();
			if (platform != OSPlatform.Linux)
			{
				ButtonCopyClipboard = AddButton("Copy to Clipboard", Icons.Streams.PadNote);
			}
			ButtonSave = AddButton("Save", Icons.Streams.Save);

			AddSeparator();
			ButtonClose = AddButton("Close Snapshot", Icons.Streams.Delete);
		}
	}
}
