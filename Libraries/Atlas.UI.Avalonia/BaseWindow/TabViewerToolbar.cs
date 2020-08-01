using Atlas.Core;
using Atlas.Resources;
using Atlas.UI.Avalonia.Tabs;
using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Atlas.UI.Avalonia
{
	public class TabViewerToolbar : TabControlToolbar
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

		private TabViewer tabViewer;

		public RelayCommand commandBindingBack;
		public RelayCommand commandBindingForward;

		public TabViewerToolbar(TabViewer tabViewer) : base(null)
		{
			this.tabViewer = tabViewer;
			InitializeControls();
		}

		// don't want to reload this because 
		private void InitializeControls()
		{
			/*
			var commandBinding = new CommandBinding(
				ApplicationCommands.Open,
				OpenCmdExecuted,
				OpenCmdCanExecute);

			CommandBindings.Add(commandBinding);

			//button.Command = commandBinding;*/

			//var commandBack = new RoutedCommand("Back", GetType());

			commandBindingBack = new RelayCommand(
				(obj) => CommandBackCanExecute(obj),
				(obj) => tabViewer.SeekBackward());

			commandBindingForward = new RelayCommand(
				(obj) => CommandForwardCanExecute(obj),
				(obj) => tabViewer.SeekForward());

			//project.navigator.CanSeekBackwardOb
			//CommandBinder.
			//CommandBindings.Add(commandBindingBack);


			//var gesture2 = new KeyGesture { Key = Key.B, Modifiers = InputModifiers.Control };
			//HotKeyManager.SetHotKey(button, gesture1);

			// gray color 3289C7
			buttonBack = AddButton("Back (Alt+Left)", Icons.Streams.Back, commandBindingBack);
			buttonForward = AddButton("Forward (Alt+Right)", Icons.Streams.Forward, commandBindingForward);

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
			var buttonBack = new ToolbarButton2()
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
			/*var buttonForward = new Button()
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
			tabViewer.Reload();
		}

		private bool CommandBackCanExecute(object obj)
		{
			return true;
			//return project.Navigator.CanSeekBackward;
		}

		private bool CommandForwardCanExecute(object obj)
		{
			return true;
			//return project.Navigator.CanSeekForward;
		}

		public void SetSnapshotVisible(bool visible)
		{
			buttonSnapshotCancel.IsVisible = visible;
		}
	}
}
