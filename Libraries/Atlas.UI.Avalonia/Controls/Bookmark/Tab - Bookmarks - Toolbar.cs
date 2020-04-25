using Atlas.Resources;
using Atlas.UI.Avalonia.Tabs;

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
			//buttonCopyClipBoard = AddButton("Copy to Clipboard", Icons.Streams.PadNote);
			buttonLink = AddButton("Link - Copy to Clipboard", Icons.Streams.Link);
			buttonImport = AddButton("Import Link from Clipboard", Icons.Streams.Import);

			//buttonSearch = AddButton("Search", Icons.Streams.Search);
			//AddSeparator();
			//AddSeparator();
			//AddButton("Save", Icons.Streams.Save);
			//AddSeparator();
			//textBlockStatus = AddLabel("Status");
		}
	}
}

/*
Probably convert these to IObservables? and replace RelayCommand
*/
