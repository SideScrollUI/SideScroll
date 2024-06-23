using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using SideScroll.UI.Avalonia.Themes;
using System.Reflection;
using System.Windows.Input;

namespace SideScroll.UI.Avalonia.Controls.Toolbar;

public class ToolbarSeparator : Border;

public class TabControlToolbar : Grid, IDisposable
{
	protected override Type StyleKeyOverride => typeof(TabControlToolbar);

	public static Thickness DefaultMargin = new(6, 2);

	public readonly TabInstance? TabInstance;

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
	}

	public void LoadToolbar(TabToolbar toolbar)
	{
		var properties = toolbar.GetType().GetVisibleProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			var propertyValue = propertyInfo.GetValue(toolbar);
			if (propertyValue != null && propertyInfo.GetCustomAttribute<SeparatorAttribute>() != null)
			{
				AddSeparator();
			}

			if (propertyValue is ToolToggleButton toolToggleButton)
			{
				AddToggleButton(toolToggleButton);
			}
			else if (propertyValue is ToolButton toolButton)
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
				{
					AddFill();
				}

				AddLabel(text);
			}
		}

		if (toolbar.Buttons.Count > 0)
		{
			AddSeparator();
			foreach (var toolButton in toolbar.Buttons)
			{
				if (toolButton is ToolToggleButton toolToggleButton)
				{
					AddToggleButton(toolToggleButton);
				}
				else
				{
					AddButton(toolButton);
				}
			}
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

	public ToolbarButton AddButton(string tooltip, IResourceView imageResource, string? label = null, ICommand? command = null)
	{
		var button = new ToolbarButton(this, tooltip, imageResource, label, command);
		AddControl(button);
		return button;
	}

	public ToolbarButton AddButton(ToolButton toolButton)
	{
		var button = new ToolbarButton(this, toolButton);
		AddControl(button);
		return button;
	}

	public ToolbarToggleButton AddToggleButton(string tooltip, IResourceView onImageResource, IResourceView offImageResource, bool isChecked, string? label = null, ICommand? command = null)
	{
		var button = new ToolbarToggleButton(this, tooltip, onImageResource, offImageResource, isChecked, label, command);
		AddControl(button);
		return button;
	}

	public ToolbarToggleButton AddToggleButton(ToolToggleButton toolButton)
	{
		var button = new ToolbarToggleButton(this, toolButton);
		AddControl(button);
		return button;
	}

	public ToolbarRadioButton AddRadioButton(string text)
	{
		var radioButton = new ToolbarRadioButton(text);
		AddControl(radioButton);
		return radioButton;
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

		AddControl(new ToolbarSeparator());
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

	// Read Only
	public TextBox AddLabelText(string text, bool fill = false)
	{
		var textBox = new TextBox
		{
			Text = text,
			TextWrapping = TextWrapping.NoWrap,
			VerticalAlignment = VerticalAlignment.Center,
			IsReadOnly = true,
			Margin = DefaultMargin,
			BorderThickness = new Thickness(0),
			BorderBrush = Brushes.Transparent,
			Background = Brushes.Transparent,
			Foreground = SideScrollTheme.ToolbarLabelForeground,
			//CaretBrush = new SolidColorBrush(Theme.GridSelectedBackgroundColor), // todo: enable with next version?
		};
		// Fluent
		textBox.Resources.Add("TextControlBackgroundPointerOver", textBox.Background);
		textBox.Resources.Add("TextControlBackgroundFocused", textBox.Background);
		textBox.Resources.Add("TextReadOnlyBackgroundBrush", textBox.Background);

		AddControl(textBox, fill);
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

	public IEnumerable<ToolbarButton> GetHotKeyButtons()
	{
		return Children
			.Where(c => c is ToolbarButton button && button.HotKey != null)
			.Select(c => (ToolbarButton)c);
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
	public ToolbarTextBlock(string text = "")
	{
		Text = text;
		Margin = TabControlToolbar.DefaultMargin;
		TextWrapping = TextWrapping.NoWrap;
		VerticalAlignment = VerticalAlignment.Center;
	}
}

public class ToolbarHeaderTextBlock(string text = "") : ToolbarTextBlock(text);

public class ToolbarRadioButton : RadioButton
{
	protected override Type StyleKeyOverride => typeof(RadioButton);

	public ToolbarRadioButton(string text = "")
	{
		Foreground = SideScrollTheme.ToolbarLabelForeground;
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

	private bool? _prevCanExecute;
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
