using Atlas.Core;
using Atlas.Extensions;
using Atlas.Resources;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Reflection;
using System.Windows.Input;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlToolbar : Grid, IDisposable
{
	public static Thickness DefaultMargin = new(6, 2);

	public TabInstance? TabInstance;

	public TabControlToolbar(TabInstance? tabInstance, TabToolbar? toolbar = null)
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

		Background = AtlasTheme.ToolbarButtonBackground;
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

	public ToolbarButton AddButton(string tooltip, ResourceView imageResource, ICommand? command = null)
	{
		var button = new ToolbarButton(this, null, tooltip, imageResource, command);
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

		PropertyInfo propertyInfo = toolComboBox.GetType().GetProperty(nameof(IToolComboBox.SelectedObject))!;
		var comboBox = new TabControlFormattedComboBox(new ListProperty(toolComboBox, propertyInfo), toolComboBox.GetItems());
		AddControl(comboBox);
		return comboBox;
	}

	public void AddSeparator()
	{
		// For optional null controls
		if (Children.Count == 0)
			return;

		Panel panel = new()
		{
			Background = AtlasTheme.ToolbarButtonSeparator,
			Width = 1,
			Margin = new Thickness(4, 2),
		};
		AddControl(panel);
	}

	// For right aligning
	public void AddFill()
	{
		Panel panel = new()
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
			Background = AtlasTheme.ToolbarButtonBackground,
			Foreground = AtlasTheme.TitleForeground,
			//CaretBrush = new SolidColorBrush(Theme.GridSelectedBackgroundColor), // todo: enable with next version?
		};
		// Fluent
		textBox.Resources.Add("TextControlBackgroundPointerOver", textBox.Background);
		textBox.Resources.Add("TextControlBackgroundFocused", textBox.Background);
		textBox.Resources.Add("TextBackgroundDisabledBrush", textBox.Background);

		AddControl(textBox);
		return textBox;
	}

	// Editable
	public TextBox AddText(string text, int minWidth)
	{
		var textBox = new ToolbarTextBox(text)
		{
			MinWidth = minWidth,
			Margin = DefaultMargin,
		};

		AddControl(textBox);

		return textBox;
	}

	public void Dispose()
	{
		foreach (Control control in Children)
		{
			if (control is IDisposable disposable)
				disposable.Dispose();
		}
	}
}

public class ToolbarTextBlock : TextBlock
{
	protected override Type StyleKeyOverride => typeof(TextBlock);

	public ToolbarTextBlock(string text = "")
	{
		Foreground = AtlasTheme.BackgroundText;
		Text = text;
		Margin = TabControlToolbar.DefaultMargin;
		TextWrapping = TextWrapping.NoWrap;
		VerticalAlignment = VerticalAlignment.Center;
	}
}

public class ToolbarRadioButton : RadioButton
{
	protected override Type StyleKeyOverride => typeof(RadioButton);

	public ToolbarRadioButton(string text = "")
	{
		Foreground = AtlasTheme.TitleForeground;
		Content = text;
		Margin = TabControlToolbar.DefaultMargin;
		VerticalAlignment = VerticalAlignment.Center;
	}
}

// todo: replace with version that uses IObservable
public class RelayCommand : ICommand
{
	public readonly Func<object?, bool> CanExecuteFunc;
	public readonly Action<object?> ExecuteAction;

	public RelayCommand(Func<object?, bool>? canExecute = null, Action<object?>? execute = null)
	{
		CanExecuteFunc = canExecute ?? (_ => true);
		ExecuteAction = execute ?? (_ => { });
	}

	public event EventHandler? CanExecuteChanged;

	private bool? _prevCanExecute = null;
	public bool CanExecute(object? parameter)
	{
		var ce = CanExecuteFunc(parameter);
		if (CanExecuteChanged != null && (!_prevCanExecute.HasValue || ce != _prevCanExecute))
		{
			_prevCanExecute = ce;
			CanExecuteChanged(this, EventArgs.Empty);
		}

		return ce;
	}

	public void Execute(object? parameter)
	{
		ExecuteAction(parameter);
	}
}
