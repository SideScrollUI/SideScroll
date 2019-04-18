using Atlas.GUI.Avalonia.Tabs;
using Atlas.Resources;
using Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabControlSearchToolbar : TabControlToolbar
	{
		public Button buttonSearch;
		public Button buttonLoadAdd;
		public Button buttonLoadNext;

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
			AddButton("Save", Assets.Streams.Save);
			textBlockStatus = AddLabel("Status");
		}
	}
}

/*
Probably convert these to IObservables? and replace RelayCommand
*/
