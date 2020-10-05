using Atlas.Resources;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Tabs;
using Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabControlSearchToolbar : TabControlToolbar
	{
		public ToolbarButton ButtonSearch;
		public ToolbarButton ButtonLoadAdd;
		public ToolbarButton ButtonLoadNext;
		public ToolbarButton ButtonSleep;
		public ToolbarButton ButtonCopyClipBoard;

		public TextBox TextBoxStatus;

		public TabControlSearchToolbar(TabInstance tabInstance) : base(tabInstance)
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			//project.navigator.CanSeekBackwardOb
			//CommandBinder.
			//CommandBindings.Add(commandBindingBack);

			ButtonSearch = AddButton("Search", Icons.Streams.Search);
			ButtonLoadNext = AddButton("Next", Icons.Streams.Forward);
			ButtonSleep = AddButton("Sleep", Icons.Streams.Refresh);
			AddSeparator();
			ButtonLoadAdd = AddButton("Add", Icons.Streams.Add);
			AddSeparator();
			AddButton("Save", Icons.Streams.Save);
			AddSeparator();
			ButtonCopyClipBoard = AddButton("Copy to Clipboard", Icons.Streams.PadNote);
			TextBoxStatus = AddLabelText("Status");
		}
	}
}

/*
Probably convert these to IObservables? and replace RelayCommand
*/
