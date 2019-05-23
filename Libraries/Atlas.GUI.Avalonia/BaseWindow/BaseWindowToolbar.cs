using Atlas.GUI.Avalonia.Tabs;
using Atlas.Resources;
using Atlas.Tabs;
using Avalonia.Controls;
using System;

namespace Atlas.GUI.Avalonia
{
	public class BaseWindowToolbar : TabControlToolbar
	{
		public Button buttonLoadNext;
		public Button buttonLink;
		public Button buttonImport;
		//public Project project;
		private BaseWindow baseWindow;

		public BaseWindowToolbar(BaseWindow baseWindow)
		{
			this.baseWindow = baseWindow;
			InitializeControls();
		}

		// don't want to reload this because 
		private void InitializeControls()
		{
			/*
			CommandBinding commandBinding = new CommandBinding(
				ApplicationCommands.Open,
				OpenCmdExecuted,
				OpenCmdCanExecute);

			CommandBindings.Add(commandBinding);

			Button button = new Button();
			button.Content = "Back";
			//button.Command = commandBinding;

			toolBar.Items.Add(button);
			stackPanel.Children.Add(toolBar);*/

			//RoutedCommand commandBack = new RoutedCommand("Back", GetType());

			RelayCommand commandBindingBack = new RelayCommand(
				(obj) => CommandBackCanExecute(obj),
				(obj) => CommandBackExecute(obj));

			RelayCommand commandBindingForward = new RelayCommand(
				(obj) => CommandForwardCanExecute(obj),
				(obj) => CommandForwardExecute(obj));

			//project.navigator.CanSeekBackwardOb
			//CommandBinder.
			//CommandBindings.Add(commandBindingBack);


			//var gesture2 = new KeyGesture { Key = Key.B, Modifiers = InputModifiers.Control };
			//HotKeyManager.SetHotKey(button, gesture1);

			// gray color 3289C7
			Button buttonBack = AddButton("Back", Assets.Streams.Back, commandBindingBack);
			Button buttonForward = AddButton("Forward", Assets.Streams.Forward, commandBindingForward);

			

			/*
			ToolbarButton2 buttonBack = new ToolbarButton2()
			{
				Content = "<-",
				//ToolTip = "Back",
				//Content = imageBack,
				//Command = commandBindingBack,
			};*/
			// Requires ReactiveUI import, can we get a version that imports?
			//var commandBack = ReactiveCommand.Create(() => ButtonBack_Click(null, null));
			//buttonBack.Bind(Class1.DoubleValueProperty, new Binding("[0]", BindingMode.TwoWay) { Source = source });
			//buttonBack.Click += ButtonBack_Click;
			/*Button buttonForward = new Button()
			{
				Content = "->",
				//ToolTip = "Forward",
				//Content = imageForward,
				//Command = commandBindingForward.Command,
				//Command = commandBack,
			};*/
			//buttonForward.Click += ButtonForward_Click;
		}

		public void AddClipBoardButtons()
		{
			AddSeparator();
			buttonLink = AddButton("Link - Copy to Clipboard", Assets.Streams.Link);
			buttonImport = AddButton("Import from Clipboard", Assets.Streams.Import);
		}

		private bool CommandBackCanExecute(object obj)
		{
			return true;
			//return project.Navigator.CanSeekBackward;
		}

		private void CommandBackExecute(object obj)
		{
			Bookmark bookmark = baseWindow.project.Navigator.SeekBackward();
			if (bookmark != null)
				baseWindow.tabView.tabInstance.SelectBookmark(bookmark.tabBookmark);
		}

		private void ButtonBack_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Bookmark bookmark = baseWindow.project.Navigator.SeekBackward();
			if (bookmark != null)
				baseWindow.tabView.tabInstance.SelectBookmark(bookmark.tabBookmark);
		}

		private bool CommandForwardCanExecute(object obj)
		{
			return true;
			//return project.Navigator.CanSeekForward;
		}

		private void CommandForwardExecute(object obj)
		{
			Bookmark bookmark = baseWindow.project.Navigator.SeekForward();
			if (bookmark != null)
				baseWindow.tabView.tabInstance.SelectBookmark(bookmark.tabBookmark);
		}

		private void ButtonForward_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Bookmark bookmark = baseWindow.project.Navigator.SeekForward();
			if (bookmark != null)
				baseWindow.tabView.tabInstance.SelectBookmark(bookmark.tabBookmark);
		}
	}

	// can't get derived class to work
	/*public class ToolbarButton : Button
	{
		public ToolbarButton()
		{
			Background = new SolidColorBrush(Colors.Blue);

			//this.PointerEnter += ToolbarButton_PointerEnter;
			//this.PointerLeave += ToolbarButton_PointerLeave;
		}

		private void ToolbarButton_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			this.Background = new SolidColorBrush(Colors.Blue);
			//this.InvalidateVisual();
		}

		private void ToolbarButton_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			this.Background = new SolidColorBrush(Colors.Green);
			//this.InvalidateVisual();
		}
	}*/
}
