using Atlas.Resources;
using Atlas.UI.Avalonia.Tabs;
using System;

namespace Atlas.UI.Avalonia
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
			ButtonCopyClipboard = AddButton("Copy to Clipboard", Icons.Streams.Clipboard);
			ButtonSave = AddButton("Save", Icons.Streams.Save);

			AddSeparator();
			ButtonClose = AddButton("Close Snapshot", Icons.Streams.Delete);
		}
	}
}
