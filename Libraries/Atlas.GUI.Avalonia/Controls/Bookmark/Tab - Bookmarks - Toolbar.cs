using Atlas.GUI.Avalonia.Tabs;
using Atlas.Resources;
using Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabControlBookmarksToolbar : TabControlToolbar
	{
		//public Button buttonSearch;
		public Button buttonAdd;
		public Button buttonCopyClipBoard;

		//public TextBlock textBlockStatus;

		public TabControlBookmarksToolbar()
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			buttonAdd = AddButton("Add", Assets.Streams.Add);
			buttonCopyClipBoard = AddButton("Copy to Clipboard", Assets.Streams.PadNote);

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
