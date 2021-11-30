using Atlas.Core;
using Atlas.Resources;
using Atlas.UI.Avalonia.Controls;
using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Atlas.UI.Avalonia
{
	public class TabViewerToolbar : TabControlToolbar
	{
		public TabViewer TabViewer;

		public ToolbarButton ButtonBack;
		public ToolbarButton ButtonForward;
		public ToolbarButton ButtonLink;
		public ToolbarButton ButtonImport;
		public ToolbarButton ButtonRefresh;

		public RelayCommand CommandBindingBack;
		public RelayCommand CommandBindingForward;

		public TabViewerToolbar(TabViewer tabViewer) : base(null)
		{
			TabViewer = tabViewer;
			InitializeControls();
		}

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

			CommandBindingBack = new RelayCommand(
				(obj) => CommandBackCanExecute(obj),
				(obj) => TabViewer.SeekBackward());

			CommandBindingForward = new RelayCommand(
				(obj) => CommandForwardCanExecute(obj),
				(obj) => TabViewer.SeekForward());

			//project.navigator.CanSeekBackwardOb
			//CommandBinder.
			//CommandBindings.Add(commandBindingBack);


			//var gesture2 = new KeyGesture { Key = Key.B, Modifiers = InputModifiers.Control };
			//HotKeyManager.SetHotKey(button, gesture1);

			// gray color 3289C7
			ButtonBack = AddButton("Back (Alt+Left)", Icons.Streams.Back, CommandBindingBack);
			ButtonForward = AddButton("Forward (Alt+Right)", Icons.Streams.Forward, CommandBindingForward);

			AddSeparator();
			ButtonRefresh = AddButton("Refresh (Ctrl+R)", Icons.Streams.Refresh);
			//buttonRefresh.Add();
			ButtonRefresh.Add(Refresh);

			AddSeparator();
			ButtonLink = AddButton("Link - Copy to Clipboard", Icons.Streams.Link);
			ButtonImport = AddButton("Import Link from Clipboard", Icons.Streams.Import);

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

		private void Refresh(Call call)
		{
			TabViewer.Reload();
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
	}
}
