using Atlas.Resources;
using Atlas.UI.Avalonia.Tabs;
using Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabControlSearchToolbar : TabControlToolbar
	{
		public ToolbarButton buttonSearch;
		public ToolbarButton buttonLoadAdd;
		public ToolbarButton buttonLoadNext;
		public ToolbarButton buttonSleep;
		public ToolbarButton buttonCopyClipBoard;

		public TextBox textBoxStatus;

		public TabControlSearchToolbar()
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			//project.navigator.CanSeekBackwardOb
			//CommandBinder.
			//CommandBindings.Add(commandBindingBack);

			buttonSearch = AddButton("Search", Icons.Streams.Search);
			buttonLoadNext = AddButton("Next", Icons.Streams.Forward);
			buttonSleep = AddButton("Sleep", Icons.Streams.Refresh);
			AddSeparator();
			buttonLoadAdd = AddButton("Add", Icons.Streams.Add);
			AddSeparator();
			AddButton("Save", Icons.Streams.Save);
			AddSeparator();
			buttonCopyClipBoard = AddButton("Copy to Clipboard", Icons.Streams.PadNote);
			textBoxStatus = AddLabelText("Status");
		}
	}
}

/*
Probably convert these to IObservables? and replace RelayCommand
*/
