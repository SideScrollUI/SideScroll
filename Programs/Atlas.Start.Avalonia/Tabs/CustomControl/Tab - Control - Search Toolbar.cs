using Atlas.GUI.Avalonia.Tabs;
using Atlas.Resources;
using Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabControlSearchToolbar : TabControlToolbar
	{
		public ToolbarButton buttonSearch;
		public ToolbarButton buttonLoadAdd;
		public ToolbarButton buttonLoadNext;
		public ToolbarButton buttonCopyClipBoard;

		public TextBlock textBlockStatus;

		public TabControlSearchToolbar()
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			//project.navigator.CanSeekBackwardOb
			//CommandBinder.
			//CommandBindings.Add(commandBindingBack);

			buttonSearch = AddButton("Search", Assets.Streams.Search);
			buttonLoadNext = AddButton("Next", Assets.Streams.Forward);
			AddSeparator();
			buttonLoadAdd = AddButton("Add", Assets.Streams.Add);
			AddSeparator();
			AddButton("Save", Assets.Streams.Save);
			AddSeparator();
			buttonCopyClipBoard = AddButton("Copy to Clipboard", Assets.Streams.PadNote);
			textBlockStatus = AddLabel("Status");
		}
	}
}

/*
Probably convert these to IObservables? and replace RelayCommand
*/
