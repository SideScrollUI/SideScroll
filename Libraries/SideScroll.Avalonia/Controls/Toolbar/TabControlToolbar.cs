using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Attributes;
using SideScroll.Avalonia.Themes;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.Toolbar;

/// <summary>A visual separator used between toolbar buttons.</summary>
public class ToolbarSeparator : Border;

/// <summary>
/// A horizontal toolbar grid that hosts buttons, toggle buttons, combo boxes, labels, and separators,
/// with optional data binding to a <see cref="TabToolbar"/> model.
/// </summary>
public class TabControlToolbar : Grid, IDisposable
{
	protected override Type StyleKeyOverride => typeof(TabControlToolbar);

	/// <summary>Gets or sets the default margin applied to toolbar controls.</summary>
	public static Thickness DefaultMargin { get; set; } = new(6, 2);

	/// <summary>Gets or sets the tab instance used for task context when invoking button actions.</summary>
	public TabInstance? TabInstance { get; set; }

	public TabControlToolbar(TabInstance? tabInstance = null, TabToolbar? toolbar = null)
	{
		TabInstance = tabInstance;

		RowDefinitions = new RowDefinitions("Auto");

		if (toolbar != null)
		{
			LoadToolbar(toolbar);
		}
	}

	/// <summary>Populates the toolbar by reflecting the properties of the given <see cref="TabToolbar"/> model.</summary>
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

	/// <summary>Adds a control to the next available column, optionally using a star-sized fill column.</summary>
	public void AddControl(Control control, bool fill = false)
	{
		Grid.SetColumn(control, ColumnDefinitions.Count);
		if (fill)
		{
			ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
		}
		else
		{
			ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
		}
		Children.Add(control);
	}

	/// <summary>Adds a toolbar button with an explicit label overload.</summary>
	public ToolbarButton AddButton(string tooltip, IResourceView imageResource, string? label)
	{
		var button = new ToolbarButton(this, tooltip, imageResource, null, label);
		AddControl(button);
		return button;
	}

	/// <summary>Adds a toolbar button with optional icon size, label, and color update settings.</summary>
	public ToolbarButton AddButton(string tooltip, IResourceView imageResource, double? iconSize = null, string? label = null, bool updateIconColors = true)
	{
		var button = new ToolbarButton(this, tooltip, imageResource, iconSize, label, updateIconColors);
		AddControl(button);
		return button;
	}

	/// <summary>Adds a toolbar button from a <see cref="ToolButton"/> model.</summary>
	public ToolbarButton AddButton(ToolButton toolButton)
	{
		var button = new ToolbarButton(this, toolButton);
		AddControl(button);
		return button;
	}

	/// <summary>Adds a toggle button with separate on/off image resources.</summary>
	public ToolbarToggleButton AddToggleButton(string tooltip, IResourceView onImageResource, IResourceView offImageResource, bool isChecked, string? label = null)
	{
		var button = new ToolbarToggleButton(this, tooltip, onImageResource, offImageResource, isChecked, label);
		AddControl(button);
		return button;
	}

	/// <summary>Adds a toggle button from a <see cref="ToolToggleButton"/> model.</summary>
	public ToolbarToggleButton AddToggleButton(ToolToggleButton toolButton)
	{
		var button = new ToolbarToggleButton(this, toolButton);
		AddControl(button);
		return button;
	}

	/// <summary>Adds a radio button with the given text label.</summary>
	public ToolbarRadioButton AddRadioButton(string text, bool isChecked = false)
	{
		var radioButton = new ToolbarRadioButton(text)
		{
			IsChecked = isChecked,
		};
		AddControl(radioButton);
		return radioButton;
	}

	/// <summary>Adds a label text block followed by a formatted combo box for the given combo box model.</summary>
	public TabFormattedComboBox AddComboBox(IToolComboBox toolComboBox)
	{
		var textBlock = new ToolbarTextBlock(toolComboBox.Label);
		AddControl(textBlock);

		PropertyInfo propertyInfo = toolComboBox.GetType().GetProperty(nameof(IToolComboBox.SelectedObject))!;
		var comboBox = new TabFormattedComboBox(new ListProperty(toolComboBox, propertyInfo), toolComboBox.GetItems());
		AddControl(comboBox);
		return comboBox;
	}

	/// <summary>Adds a separator, skipping it if the toolbar has no existing controls.</summary>
	public void AddSeparator()
	{
		// For optional null controls
		if (Children.Count == 0)
			return;

		AddControl(new ToolbarSeparator());
	}

	/// <summary>Adds a transparent stretch panel that pushes subsequent toolbar controls to the right.</summary>
	public void AddFill()
	{
		Panel panel = new()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			Background = Brushes.Transparent, // Custom Title Toolbar requires this for dragging
			IsHitTestVisible = false, // Custom Title Toolbar requires this for dragging
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
			Padding = new Thickness(10, 8, 6, 2),
			BorderThickness = new Thickness(0),
			BorderBrush = Brushes.Transparent,
			Background = Brushes.Transparent,
			Foreground = SideScrollTheme.ToolbarLabelForeground,
			//CaretBrush = new SolidColorBrush(Theme.GridSelectedBackgroundColor), // todo: enable with next version?
		};
		// Fluent
		textBox.Resources.Add("TextControlBackgroundPointerOver", textBox.Background);
		textBox.Resources.Add("TextControlBackgroundFocused", textBox.Background);
		textBox.Resources.Add("TextControlBackgroundReadOnlyBrush", textBox.Background);

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
			{
				disposable.Dispose();
			}
		}
	}
}

/// <summary>A non-wrapping text block styled for use in a toolbar.</summary>
public class ToolbarTextBlock : TextBlock
{
	public ToolbarTextBlock(string text = "")
	{
		Text = text;
		Margin = new Thickness(6, 2, 6, 0);
		TextWrapping = TextWrapping.NoWrap;
		VerticalAlignment = VerticalAlignment.Center;
	}
}

/// <summary>A bold header text block styled for use in a toolbar.</summary>
public class ToolbarHeaderTextBlock(string text = "") : ToolbarTextBlock(text);

/// <summary>A radio button styled for horizontal placement in a toolbar.</summary>
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
