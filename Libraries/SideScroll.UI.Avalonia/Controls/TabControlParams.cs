using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Tabs;
using SideScroll.UI.Avalonia.Utilities;
using SideScroll.UI.Avalonia.View;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SideScroll.UI.Avalonia.Controls;

public class TabHeader : Border
{
	public TabHeader(string text)
	{
		Child = new TextBlock()
		{
			Text = text,
		};
	}
}

public class TabSeparator : Border;

public class TabControlParams : Grid, IValidationControl
{
	public static int ControlMaxWidth { get; set; } = 2000;
	public static int ControlMaxHeight { get; set; } = 400;

	public object? Object;

	private readonly Dictionary<ListProperty, Control> _propertyControls = [];

	public override string? ToString() => Object?.ToString();

	public TabControlParams(object? obj, bool autoGenerateRows = true, string columnDefinitions = "Auto,*")
	{
		Object = obj;

		InitializeControls(columnDefinitions);

		if (autoGenerateRows)
			LoadObject(obj);
	}

	private void InitializeControls(string columnDefinitions)
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		ColumnDefinitions = new ColumnDefinitions(columnDefinitions);

		Margin = new Thickness(6);

		MinWidth = 100;
		MaxWidth = 2000;
	}

	private void ClearControls()
	{
		Children.Clear();
		RowDefinitions.Clear();
	}

	public void LoadObject(object? obj)
	{
		ClearControls();

		if (obj == null) return;

		AddSummary();
		AddPropertyControls(obj);
	}

	private void AddPropertyControls(object obj)
	{
		ItemCollection<ListProperty> properties = ListProperty.Create(obj);

		foreach (ListProperty property in properties)
		{
			int columnIndex = property.GetCustomAttribute<ColumnIndexAttribute>()?.Index ?? 0;
			AddColumnIndex(columnIndex + 1); // label + value controls
		}

		Control? lastControl = null;
		foreach (ListProperty property in properties)
		{
			if (property.GetCustomAttribute<HeaderAttribute>() is HeaderAttribute headerAttribute)
			{
				AddHeader(headerAttribute.Text);
			}
			else if (property.GetCustomAttribute<SeparatorAttribute>() != null)
			{
				AddSeparator();
			}

			var newControl = AddPropertyControl(property);
			if (newControl != null)
			{
				if (lastControl != null && GetRow(lastControl) != GetRow(newControl))
				{
					FillColumnSpan(lastControl);
				}
				_propertyControls[property] = newControl;
			}
			lastControl = newControl;
		}

		if (lastControl != null)
		{
			FillColumnSpan(lastControl);
		}
	}

	// Fill entire last line if available
	private void FillColumnSpan(Control lastControl)
	{
		int columnIndex = GetColumn(lastControl);
		int columnSpan = GetColumnSpan(lastControl);
		if (columnIndex + columnSpan < ColumnDefinitions.Count)
		{
			SetColumnSpan(lastControl, ColumnDefinitions.Count - columnIndex);
		}
	}

	private void AddSummary()
	{
		var summaryAttribute = Object!.GetType().GetCustomAttribute<SummaryAttribute>();
		if (summaryAttribute == null)
			return;

		AddRowDefinition();

		TabControlTextBlock textBlock = new()
		{
			Text = summaryAttribute.Summary,
			Margin = new Thickness(0, 3, 10, 3),
			VerticalAlignment = VerticalAlignment.Top,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			TextWrapping = TextWrapping.Wrap,
			MaxWidth = ControlMaxWidth,
			[ColumnSpanProperty] = 2,
		};
		Children.Add(textBlock);
	}

	public List<Control> AddObjectRow(object obj, List<PropertyInfo>? properties = null)
	{
		properties ??= obj.GetType().GetVisibleProperties();

		int rowIndex = AddRowDefinition();
		int columnIndex = 0;

		List<Control> controls = [];
		foreach (PropertyInfo propertyInfo in properties)
		{
			var property = new ListProperty(obj, propertyInfo);
			Control? control = CreatePropertyControl(property);
			if (control == null)
				continue;

			AddControl(control, columnIndex, rowIndex);
			controls.Add(control);
			columnIndex++;

			_propertyControls[property] = control;
		}
		return controls;
	}

	private int AddRowDefinition()
	{
		int rowIndex = RowDefinitions.Count;
		RowDefinition rowDefinition = new()
		{
			Height = new GridLength(1, GridUnitType.Auto),
		};
		RowDefinitions.Add(rowDefinition);
		return rowIndex;
	}

	private void AddControl(Control control, int columnIndex, int rowIndex)
	{
		AddColumnIndex(columnIndex);

		SetColumn(control, columnIndex);
		SetRow(control, rowIndex);
		Children.Add(control);
	}

	private void AddColumnIndex(int columnIndex)
	{
		while (columnIndex >= ColumnDefinitions.Count)
		{
			GridUnitType type = (ColumnDefinitions.Count % 2 == 0) ? GridUnitType.Auto : GridUnitType.Star;
			var columnDefinition = new ColumnDefinition(1, type);
			ColumnDefinitions.Add(columnDefinition);
		}
	}

	public Control? AddPropertyControl(string propertyName)
	{
		PropertyInfo propertyInfo = Object!.GetType().GetProperty(propertyName)!;
		return AddPropertyControl(new ListProperty(Object, propertyInfo));
	}

	public Control? AddPropertyControl(PropertyInfo propertyInfo)
	{
		return AddPropertyControl(new ListProperty(Object!, propertyInfo));
	}

	public Control? AddPropertyControl(ListProperty property)
	{
		int columnIndex = property.GetCustomAttribute<ColumnIndexAttribute>()?.Index ?? 0;

		Control? control = CreatePropertyControl(property);
		if (control == null)
			return null;

		property.Cachable = false;

		int rowIndex = RowDefinitions.Count;

		if (rowIndex > 0 && columnIndex > 0)
		{
			rowIndex--; // Reuse previous row
		}
		else
		{
			if (columnIndex == 0)
			{
				AddSpacer();
				rowIndex++;
			}

			RowDefinition rowDefinition = new()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			RowDefinitions.Add(rowDefinition);
		}

		TabControlTextBlock textLabel = new()
		{
			Text = property.Name,
			Margin = new Thickness(10, 7, 10, 3),
			VerticalAlignment = VerticalAlignment.Top,
			MaxWidth = ControlMaxWidth,
			[Grid.RowProperty] = rowIndex,
			[Grid.ColumnProperty] = columnIndex++,
		};
		Children.Add(textLabel);

		AddControl(control, columnIndex, rowIndex);

		_propertyControls[property] = control;

		return control;
	}

	private void AddSpacer(int height = 5)
	{
		RowDefinition spacerRow = new()
		{
			Height = new GridLength(height),
		};
		RowDefinitions.Add(spacerRow);
	}

	private static Control? CreatePropertyControl(ListProperty property)
	{
		Type type = property.UnderlyingType;

		BindListAttribute? listAttribute = type.GetCustomAttribute<BindListAttribute>();
		listAttribute ??= property.GetCustomAttribute<BindListAttribute>();

		if (property.Editable)
		{
			if (type.IsEnum || listAttribute != null)
			{
				return new TabControlFormattedComboBox(property, listAttribute?.PropertyName);
			}
			else if (typeof(DateTime).IsAssignableFrom(type))
			{
				return new TabDateTimePicker(property);
			}
		}

		if (type == typeof(bool))
		{
			return new TabControlCheckBox(property);
		}
		else if (typeof(Color).IsAssignableFrom(type))
		{
			return new TabControlColorPicker(property);
		}
		else if (typeof(string).IsAssignableFrom(type) || !typeof(IEnumerable).IsAssignableFrom(type))
		{
			return new TabControlTextBox(property);
		}

		return null;
	}

	public void AddHeader(string text)
	{
		if (Children.Count > 0)
		{
			AddSpacer(10);
		}

		TabHeader header = new(text)
		{
			[Grid.ColumnSpanProperty] = ColumnDefinitions.Count,
		};

		int rowIndex = RowDefinitions.Count;
		AddRowDefinition();
		AddControl(header, 0, rowIndex);
	}

	public void AddSeparator()
	{
		// Don't add for first item or optional null controls
		if (Children.Count == 0) return;

		TabSeparator separator = new()
		{
			[Grid.ColumnSpanProperty] = ColumnDefinitions.Count,
		};

		int rowIndex = RowDefinitions.Count;
		AddRowDefinition();
		AddControl(separator, 0, rowIndex);
	}

	// Focus first input control
	// Add [Focus] attribute if more control needed?
	public void Focus()
	{
		foreach (Control control in Children)
		{
			if (control is TextBox textBox)
			{
				textBox.Focus();
				return;
			}
		}
		base.Focus();
	}

	public void Validate()
	{
		bool valid = true;
		foreach (var propertyControl in _propertyControls)
		{
			valid = AvaloniaUtils.ValidateControl(propertyControl.Key, propertyControl.Value) && valid;
		}

		if (!valid)
		{
			throw new ValidationException("Invalid Parameters");
		}
	}
}

