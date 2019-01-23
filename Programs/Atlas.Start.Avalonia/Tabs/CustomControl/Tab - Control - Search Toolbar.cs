using Atlas.GUI.Avalonia.Tabs;
using Atlas.Resources;
using Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabControlSearchToolbar : TabControlToolbar
	{
		public Button buttonSearch;
		public Button buttonLoadNext;

		public TextBlock textBlockStatus;

		public TabControlSearchToolbar()
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			RelayCommand commandBindingSearch = new RelayCommand(
				(obj) => CommandSearchCanExecute(obj),
				(obj) => CommandSearchExecute(obj));

			RelayCommand commandBindingNext = new RelayCommand(
				(obj) => CommandNextCanExecute(obj),
				(obj) => CommandNextExecute(obj));

			//project.navigator.CanSeekBackwardOb
			//CommandBinder.
			//CommandBindings.Add(commandBindingBack);

			buttonSearch = AddButton("Search", commandBindingSearch, Assets.Streams.Search);
			buttonLoadNext = AddButton("Next", commandBindingNext, Assets.Streams.Forward);
			textBlockStatus = AddLabel("Status");
		}

		// Not currently being used. See TabScanControl
		private bool CommandSearchCanExecute(object obj)
		{
			return true;
		}

		private void CommandSearchExecute(object obj)
		{

		}

		private bool CommandNextCanExecute(object obj)
		{
			return true;
		}

		private void CommandNextExecute(object obj)
		{

		}
	}
}

/*
Probably convert these to IObservables? and replace RelayCommand
*/
