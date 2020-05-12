using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Tabs;
using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Atlas.UI.Avalonia
{
	public class BaseWindowToolbar : TabControlToolbar
	{
		public ToolbarButton buttonBack;
		public ToolbarButton buttonForward;
		public ToolbarButton buttonLink;
		public ToolbarButton buttonImport;
		public ToolbarButton buttonRefresh;

		public ToolbarButton buttonSnapshot;
		public ToolbarButton buttonSnapshotClipboard;
		public ToolbarButton buttonSnapshotEmbed;
		public ToolbarButton buttonSnapshotCancel;

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

			var commandBindingBack = new RelayCommand(
				(obj) => CommandBackCanExecute(obj),
				(obj) => CommandBackExecute(obj));

			var commandBindingForward = new RelayCommand(
				(obj) => CommandForwardCanExecute(obj),
				(obj) => CommandForwardExecute(obj));

			//project.navigator.CanSeekBackwardOb
			//CommandBinder.
			//CommandBindings.Add(commandBindingBack);


			//var gesture2 = new KeyGesture { Key = Key.B, Modifiers = InputModifiers.Control };
			//HotKeyManager.SetHotKey(button, gesture1);

			// gray color 3289C7
			buttonBack = AddButton("Back", Icons.Streams.Back, commandBindingBack);
			buttonForward = AddButton("Forward", Icons.Streams.Forward, commandBindingForward);

			AddSeparator();
			buttonRefresh = AddButton("Refresh (Ctrl+R)", Icons.Streams.Refresh);
			//buttonRefresh.Add();
			buttonRefresh.Add(ButtonRefresh_Click);

			AddSeparator();
			buttonLink = AddButton("Link - Copy to Clipboard", Icons.Streams.Link);
			buttonImport = AddButton("Import Link from Clipboard", Icons.Streams.Import);

#if DEBUG
			AddSeparator();
			buttonSnapshot = AddButton("Snapshot", Icons.Streams.Screenshot);
			buttonSnapshotCancel = AddButton("Cancel Snapshot", Icons.Streams.Delete);
			SetSnapshotVisible(false);
#endif

			// Handle in BaseWindow
			//var refreshGesture = new KeyGesture { Key = Key.F5 };
			//HotKeyManager.SetHotKey(buttonRefresh, refreshGesture);


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

		private void ButtonRefresh_Click(Call call)
		{
			baseWindow.Reload();
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

		public void SetSnapshotVisible(bool visible)
		{
			buttonSnapshotCancel.IsVisible = visible;
		}
	}

	// can't get derived class to work
	/*public class ToolbarButton : Button
	{
		public ToolbarButton()
		{
			Background = new SolidColorBrush(Colors.Blue);

			//PointerEnter += ToolbarButton_PointerEnter;
			//PointerLeave += ToolbarButton_PointerLeave;
		}

		private void ToolbarButton_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Background = new SolidColorBrush(Colors.Blue);
			//InvalidateVisual();
		}

		private void ToolbarButton_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Background = new SolidColorBrush(Colors.Green);
			//InvalidateVisual();
		}
	}*/
}
