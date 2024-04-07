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

public class ToolbarSeparator : Border { }

public class TabControlToolbar : Grid, IDisposable
{
	protected override Type StyleKeyOverride => typeof(TabControlToolbar);

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

	public ToolbarButton AddButton(string tooltip, IResourceView imageResource, ICommand? command = null, string? label = null)
	{
		var button = new ToolbarButton(this, label, tooltip, imageResource, command);
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

	public ToolbarRadioButton AddRadioButton(string text)
	{
		var radioButton = new ToolbarRadioButton(text);
		AddControl(radioButton);
		return radioButton;
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
			Foreground = AtlasTheme.ToolbarLabelForeground,
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

public class ToolbarHeaderTextBlock : ToolbarTextBlock
{
	public ToolbarHeaderTextBlock(string text = "")
		: base(text)
	{
	}
}

public class ToolbarRadioButton : RadioButton
{
	protected override Type StyleKeyOverride => typeof(RadioButton);

	public ToolbarRadioButton(string text = "")
	{
		Foreground = AtlasTheme.ToolbarLabelForeground;
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
