using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
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

namespace Atlas.UI.Avalonia.Tabs
{
	public class TabControlToolbar : Grid
	{
		public TabInstance tabInstance;

		public TabControlToolbar(TabInstance tabInstance = null, TabToolbar toolbar = null)
		{
			this.tabInstance = tabInstance;
			InitializeControls();
			if (toolbar != null)
				LoadToolbar(toolbar);
		}

		private void InitializeControls()
		{
			RowDefinitions = new RowDefinitions("Auto");
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Top;
			Background = Theme.ToolbarButtonBackground;
		}

		public void LoadToolbar(TabToolbar toolbar)
		{
			var properties = toolbar.GetType().GetVisibleProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetCustomAttribute<SeparatorAttribute>() != null)
					AddSeparator();
				var propertyValue = propertyInfo.GetValue(toolbar);
				if (propertyValue is ToolButton toolButton)
				{
					AddButton(toolButton);
				}
				else if (propertyValue is string text)
				{
					AddLabel(text);
				}
			}
			if (toolbar.Buttons.Count > 0)
			{
				AddSeparator();
				foreach (var toolButton in toolbar.Buttons)
					AddButton(toolButton);
			}
		}

		public void AddControl(Control control)
		{
			Grid.SetColumn(control, ColumnDefinitions.Count);
			ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
			Children.Add(control);
		}

		public ToolbarButton AddButton(string tooltip, Stream resource, ICommand command = null)
		{
			var button = new ToolbarButton(this, tooltip, resource, command);
			AddControl(button);
			return button;
		}

		public ToolbarButton AddButton(ToolButton toolButton)
		{
			var button = new ToolbarButton(this, toolButton.Label, toolButton.Icon);
			button.Add(toolButton.Action);
			button.AddAsync(toolButton.ActionAsync);
			AddControl(button);
			return button;
		}

		public void AddSeparator()
		{
			var panel = new Panel()
			{
				Background = Theme.ToolbarButtonSeparator,
				Width = 2,
				Margin = new Thickness(4),
			};
			AddControl(panel);
		}

		public ToolbarTextBlock AddLabel(string text = "")
		{
			var textBlock = new ToolbarTextBlock(text);
			AddControl(textBlock);
			return textBlock;
		}

		public ToolbarRadioButton AddRadioButton(string text)
		{
			var radioButton = new ToolbarRadioButton(text);
			AddControl(radioButton);
			return radioButton;
		}

		// Read Only
		public TextBox AddLabelText(string text)
		{
			var textBox = new TextBox()
			{
				Text = text,
				TextWrapping = TextWrapping.NoWrap,
				VerticalAlignment = VerticalAlignment.Center,
				IsReadOnly = true,
				Margin = new Thickness(6),
				BorderThickness = new Thickness(0),
				BorderBrush = Brushes.Transparent,
				Background = Brushes.Transparent,
				Foreground = Brushes.White,
				//CaretBrush = new SolidColorBrush(Theme.GridSelectedBackgroundColor), // todo: enable with next version?
			};

			AddControl(textBox);
			return textBox;
		}

		// Editable
		public TextBox AddText(string text, int minWidth)
		{
			var textBox = new TextBox()
			{
				//Foreground = new SolidColorBrush(Colors.Black),
				Text = text,
				MinWidth = minWidth,
				Margin = new Thickness(6),
				BorderThickness = new Thickness(1),
				BorderBrush = new SolidColorBrush(Colors.Black),
				TextWrapping = TextWrapping.NoWrap,
				VerticalAlignment = VerticalAlignment.Center,
			};

			AddControl(textBox);

			return textBox;
		}
	}

	public class ToolbarTextBlock : TextBlock, IStyleable
	{
		Type IStyleable.StyleKey => typeof(TextBlock);

		public ToolbarTextBlock(string text = "")
		{
			Foreground = Theme.TitleForeground;
			Text = text;
			Margin = new Thickness(6);
			TextWrapping = TextWrapping.NoWrap;
			VerticalAlignment = VerticalAlignment.Center;
		}
	}

	public class ToolbarRadioButton : RadioButton, IStyleable
	{
		Type IStyleable.StyleKey => typeof(RadioButton);

		public ToolbarRadioButton(string text = "")
		{
			Foreground = Theme.TitleForeground;
			Content = text;
			Margin = new Thickness(6);
			VerticalAlignment = VerticalAlignment.Center;
		}
	}

	public class ToolbarButton : Button, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(Button);

		public TabControlToolbar toolbar;
		public TaskDelegate.CallAction callAction;
		public TaskDelegateAsync.CallActionAsync callActionAsync;

		public ToolbarButton(TabControlToolbar toolbar, string tooltip, Stream stream, ICommand command = null) : base()
		{
			this.toolbar = toolbar;
			stream.Position = 0;
			var bitmap = new Bitmap(stream);

			var image = new Image()
			{
				Source = bitmap,
				//MaxWidth = 24,
				//MaxHeight = 24,
				Stretch = Stretch.None,
			};

			Content = image;
			Command = command;
			Background = Theme.ToolbarButtonBackground;
			BorderBrush = Background;
			BorderThickness = new Thickness(0);
			Margin = new Thickness(2);
			//BorderThickness = new Thickness(2),
			//Foreground = new SolidColorBrush(Theme.ButtonForegroundColor),
			//BorderBrush = new SolidColorBrush(Colors.Black),
			ToolTip.SetTip(this, tooltip);

			BorderBrush = Background;
			Click += ToolbarButton_Click;
		}

		private void ToolbarButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (toolbar.tabInstance == null)
				return;

			if (callActionAsync != null)
				toolbar.tabInstance.StartAsync(callActionAsync);
			if (callAction != null)
				toolbar.tabInstance.StartTask(callAction, false, false);
		}

		public void Add(TaskDelegate.CallAction callAction)
		{
			this.callAction = callAction;
		}

		public void AddAsync(TaskDelegateAsync.CallActionAsync callActionAsync)
		{
			this.callActionAsync = callActionAsync;
		}

		// DefaultTheme.xaml is overriding this currently
		protected override void OnPointerEnter(PointerEventArgs e)
		{
			base.OnPointerEnter(e);
			BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			Background = Theme.ToolbarButtonBackgroundHover;
		}

		protected override void OnPointerLeave(PointerEventArgs e)
		{
			base.OnPointerLeave(e);
			Background = Theme.ToolbarButtonBackground;
			BorderBrush = Background;
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
