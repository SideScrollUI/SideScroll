using Atlas.GUI.Avalonia.Tabs;
using Atlas.Resources;
using Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabControlBookmarksToolbar : TabControlToolbar
	{
		//public ToolbarButton buttonSearch;
		public ToolbarButton buttonAdd;
		public ToolbarButton buttonLink;
		public ToolbarButton buttonImport;
		//public ToolbarButton buttonCopyClipBoard;

		//public TextBlock textBlockStatus;

		public TabControlBookmarksToolbar()
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			buttonAdd = AddButton("Add", Icons.Streams.Add);
			//buttonCopyClipBoard = AddButton("Copy to Clipboard", Assets.Streams.PadNote);
			buttonLink = AddButton("Link - Copy to Clipboard", Icons.Streams.Link);
			buttonImport = AddButton("Import from Clipboard", Icons.Streams.Import);

			//buttonSearch = AddButton("Search", Assets.Streams.Search);
			//AddSeparator();
			//AddSeparator();
			//AddButton("Save", Assets.Streams.Save);
			//AddSeparator();
			//textBlockStatus = AddLabel("Status");
		}
	}
}

/*
Probably convert these to IObservables? and replace RelayCommand
*/
