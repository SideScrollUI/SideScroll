using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace Atlas.UI.Avalonia.Tabs
{
	public class TabControlToolbar : Grid
	{
		public static Thickness DefaultMargin = new Thickness(6, 3);

		public TabInstance TabInstance;

		public TabControlToolbar(TabInstance tabInstance, TabToolbar toolbar = null)
		{
			TabInstance = tabInstance;
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
				var propertyValue = propertyInfo.GetValue(toolbar);
				if (propertyValue != null && propertyInfo.GetCustomAttribute<SeparatorAttribute>() != null)
					AddSeparator();

				if (propertyValue is ToolButton toolButton)
				{
					AddButton(toolButton);
				}
				else if (propertyValue is IToolComboBox comboBox)
				{
					AddComboBox(comboBox);
				}
				else if (propertyValue is string text)
				{
					if (propertyInfo.GetCustomAttribute<RightAlignAttribute>() != null)
						AddFill();

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

		public void AddControl(Control control, bool fill = false)
		{
			Grid.SetColumn(control, ColumnDefinitions.Count);
			if (fill)
				ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
			else
				ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
			Children.Add(control);
		}

		public ToolbarButton AddButton(string tooltip, Stream resource, ICommand command = null)
		{
			var button = new ToolbarButton(this, null, tooltip, resource, command);
			AddControl(button);
			return button;
		}

		public ToolbarButton AddButton(ToolButton toolButton)
		{
			var button = new ToolbarButton(this, toolButton);
			AddControl(button);
			return button;
		}

		public TabControlFormattedComboBox AddComboBox(IToolComboBox toolComboBox)
		{
			var textBlock = new ToolbarTextBlock(toolComboBox.Label);
			AddControl(textBlock);

			PropertyInfo propertyInfo = toolComboBox.GetType().GetProperty(nameof(IToolComboBox.SelectedObject));
			var comboBox = new TabControlFormattedComboBox(new ListProperty(toolComboBox, propertyInfo))
			{
				Items = toolComboBox.GetItems(),
			};
			AddControl(comboBox);
			return comboBox;
		}

		public void AddSeparator()
		{
			// For optional null controls
			if (Children.Count == 0)
				return;

			var panel = new Panel()
			{
				Background = Theme.ToolbarButtonSeparator,
				Width = 2,
				Margin = new Thickness(2),
			};
			AddControl(panel);
		}

		// For right aligning
		public void AddFill()
		{
			var panel = new Panel()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			AddControl(panel, true);
		}

		public ToolbarTextBlock AddLabel(string text = "", bool fill = false)
		{
			var textBlock = new ToolbarTextBlock(text);
			AddControl(textBlock, fill);
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
				Margin = DefaultMargin,
				BorderThickness = new Thickness(0),
				BorderBrush = Brushes.Transparent,
				Background = Brushes.Transparent,
				Foreground = Theme.TitleForeground,
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
				Margin = DefaultMargin,
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
			Margin = TabControlToolbar.DefaultMargin;
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
			Margin = TabControlToolbar.DefaultMargin;
			VerticalAlignment = VerticalAlignment.Center;
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
