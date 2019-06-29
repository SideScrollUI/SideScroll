﻿using Atlas.GUI.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace Atlas.GUI.Avalonia.Tabs
{
	public class TabControlToolbar : Grid
	{
		public TabControlToolbar()
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			RowDefinitions = new RowDefinitions("Auto");
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Top;
			Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor);
		}

		public void AddControl(Control control)
		{
			Grid.SetColumn(control, ColumnDefinitions.Count);
			ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
			Children.Add(control);
		}

		public Button AddButton(string tooltip, Stream resource, ICommand command = null)
		{
			//command = command ?? new RelayCommand(
			//	(obj) => CommandDefaultCanExecute(obj),
			//	(obj) => CommandDefaultExecute(obj));
			var assembly = Assembly.GetExecutingAssembly();
			Bitmap bitmap;
			using (resource)
			{
				bitmap = new Bitmap(resource);
			}

			var image = new Image()
			{
				Source = bitmap,
				Width = 24,
				Height = 24,
			};

			Button button = new Button()
			{
				Content = image,
				Command = command,
				Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor),
				BorderBrush = Background,
				BorderThickness = new Thickness(0),
				Margin = new Thickness(2),
				//BorderThickness = new Thickness(2),
				//Foreground = new SolidColorBrush(Theme.ButtonForegroundColor),
				//BorderBrush = new SolidColorBrush(Colors.Black),
				[ToolTip.TipProperty] = tooltip,
			};
			button.BorderBrush = button.Background;
			button.PointerEnter += Button_PointerEnter;
			button.PointerLeave += Button_PointerLeave;

			//var button = new ToolbarButton(tooltip, command, resource);
			AddControl(button);
			return button;
		}

		// DefaultTheme.xaml is overriding this currently
		private void Button_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			button.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundHoverColor);
		}

		private void Button_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor);
			button.BorderBrush = button.Background;
		}

		/*private bool CommandDefaultCanExecute(object obj)
		{
			return true;
		}

		private void CommandDefaultExecute(object obj)
		{

		}*/

		public void AddSeparator()
		{
			Panel panel = new Panel()
			{
				Background = new SolidColorBrush(Theme.ToolbarButtonSeparatorColor),
				Width = 2,
				Margin = new Thickness(4),
			};
			AddControl(panel);
		}

		public TextBlock AddLabel(string text = "")
		{
			TextBlock textBlock = new TextBlock()
			{
				Foreground = new SolidColorBrush(Colors.White),
				Text = text,
				Margin = new Thickness(6),
				TextWrapping = global::Avalonia.Media.TextWrapping.NoWrap,
				VerticalAlignment = VerticalAlignment.Center,
			};

			AddControl(textBlock);

			return textBlock;
		}

		public TextBox AddText(string text, int minWidth)
		{
			TextBox textBox = new TextBox()
			{
				//Foreground = new SolidColorBrush(Colors.Black),
				Text = text,
				MinWidth = minWidth,
				Margin = new Thickness(6),
				BorderThickness = new Thickness(1),
				BorderBrush = new SolidColorBrush(Colors.Black),
				TextWrapping = global::Avalonia.Media.TextWrapping.NoWrap,
				VerticalAlignment = VerticalAlignment.Center,
			};

			AddControl(textBox);

			return textBox;
		}
	}

	// not working yet :(
	public class ToolbarButton : Button, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(Button);

		public ToolbarButton(string tooltip, ICommand command, Stream resource)
		{
			Bitmap bitmap;
			using (resource)
			{
				bitmap = new Bitmap(resource);
			}

			var image = new Image()
			{
				Source = bitmap,
			};

			Button button = new Button()
			{
				Content = image,
				Command = command,
				Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor),
				BorderBrush = Background,
				BorderThickness = new Thickness(0),
				Margin = new Thickness(2),
				//BorderThickness = new Thickness(2),
				//Foreground = new SolidColorBrush(Theme.ButtonForegroundColor),
				//BorderBrush = new SolidColorBrush(Colors.Black),
				[ToolTip.TipProperty] = tooltip,
			};
			button.BorderBrush = button.Background;
		}

		// DefaultTheme.xaml is overriding this currently
		protected override void OnPointerEnter(PointerEventArgs e)
		{
			base.OnPointerEnter(e);
			this.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			this.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundHoverColor);
		}

		protected override void OnPointerLeave(PointerEventArgs e)
		{
			base.OnPointerLeave(e);
			this.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor);
			this.BorderBrush = this.Background;
		}
	}

	// todo: replace with version that uses IObservable
	public class RelayCommand : ICommand
	{
		readonly Func<object, bool> canExecute;
		readonly Action<object> execute;

		public RelayCommand(Func<object, bool> canExecute = null, Action<object> execute = null)
		{
			this.canExecute = canExecute ?? (_ => true);
			this.execute = execute ?? (_ => { });
		}

		public event EventHandler CanExecuteChanged;

		bool? prevCanExecute = null;
		public bool CanExecute(object parameter)
		{
			var ce = canExecute(parameter);
			if (CanExecuteChanged != null && (!prevCanExecute.HasValue || ce != prevCanExecute))
			{
				prevCanExecute = ce;
				CanExecuteChanged(this, EventArgs.Empty);
			}

			return ce;
		}

		public void Execute(object parameter)
		{
			execute(parameter);
		}
	}
}
