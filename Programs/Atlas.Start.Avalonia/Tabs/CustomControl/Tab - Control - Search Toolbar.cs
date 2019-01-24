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
			RelayCommand commandBindingSearch = new RelayCommand(
				(obj) => CommandSearchCanExecute(obj),
				(obj) => CommandSearchExecute(obj));

			RelayCommand commandBindingNext = new RelayCommand(
				(obj) => CommandNextCanExecute(obj),
				(obj) => CommandNextExecute(obj));

			RelayCommand commandBindingAdd = new RelayCommand(
				(obj) => CommandAddCanExecute(obj),
				(obj) => CommandAddExecute(obj));

			//project.navigator.CanSeekBackwardOb
			//CommandBinder.
			//CommandBindings.Add(commandBindingBack);

			buttonSearch = AddButton("Search", commandBindingSearch, Assets.Streams.Search);
			buttonLoadNext = AddButton("Next", commandBindingNext, Assets.Streams.Forward);
			buttonLoadAdd = AddButton("Add", commandBindingAdd, Assets.Streams.Add);
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

		private bool CommandAddCanExecute(object obj)
		{
			return true;
		}

		private void CommandAddExecute(object obj)
		{

		}
	}
}

/*
Probably convert these to IObservables? and replace RelayCommand
*/
