using Atlas.GUI.Avalonia;
using Atlas.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace Atlas.GUI.Avalonia.Tabs
{
	public class TabControlToolbar : StackPanel
	{
		public TabControlToolbar()
		{
			InitializeControls();
		}

		// don't want to reload this because 
		private void InitializeControls()
		{
			Orientation = Orientation.Horizontal;
			//ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,Auto");
			//RowDefinitions = new RowDefinitions("Auto"); // Header, Body
			//HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Top;
			Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor);
		}

		public Button AddButton(string name, ICommand command, Stream resource)
		{
			var assembly = Assembly.GetExecutingAssembly();
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
				[ToolTip.TipProperty] = name,
			};
			button.BorderBrush = button.Background;
			button.PointerEnter += Button_PointerEnter;
			button.PointerLeave += Button_PointerLeave;

			this.Children.Add(button);

			return button;
		}

		public void AddSeparator()
		{
			Panel panel = new Panel()
			{
				Background = new SolidColorBrush(Theme.ToolbarButtonSeparatorColor),
				Width = 2,
				Margin = new Thickness(4),
			};
			this.Children.Add(panel);
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

		public TextBlock AddLabel(string text)
		{
			TextBlock textBlock = new TextBlock()
			{
				Foreground = new SolidColorBrush(Colors.White),
				Text = text,
				Margin = new Thickness(6),
				TextWrapping = global::Avalonia.Media.TextWrapping.NoWrap,
				VerticalAlignment = VerticalAlignment.Center,
			};

			this.Children.Add(textBlock);

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

			this.Children.Add(textBox);

			return textBox;
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
