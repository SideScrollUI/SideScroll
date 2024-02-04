using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Themes;
using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlParams : Grid, IValidationControl
{
	public const int ControlMaxWidth = 500;
	public const int ControlMaxHeight = 400;

	public object? Object;

	private Dictionary<ListProperty, Control> _propertyControls = new();

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

		TextBlock textBlock = new()
		{
			Text = summaryAttribute.Summary,
			FontSize = 14,
			Margin = new Thickness(0, 3, 10, 3),
			Foreground = AtlasTheme.BackgroundText,
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

		List<Control> controls = new();
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
				RowDefinition spacerRow = new()
				{
					Height = new GridLength(5),
				};
				RowDefinitions.Add(spacerRow);
				rowIndex++;
			}

			RowDefinition rowDefinition = new()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			RowDefinitions.Add(rowDefinition);
		}

		TextBlock textLabel = new()
		{
			Text = property.Name,
			Margin = new Thickness(10, 7, 10, 3),
			Foreground = AtlasTheme.BackgroundText,
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

	private Control? CreatePropertyControl(ListProperty property)
	{
		Type type = property.UnderlyingType;

		BindListAttribute? listAttribute = type.GetCustomAttribute<BindListAttribute>();
		listAttribute ??= property.GetCustomAttribute<BindListAttribute>();

		if (type == typeof(bool))
		{
			return new TabControlCheckBox(property);
		}
		else if (type.IsEnum || listAttribute != null)
		{
			return new TabControlComboBox(property, listAttribute?.PropertyName);
		}
		else if (typeof(DateTime).IsAssignableFrom(type) && property.Editable)
		{
			return new TabDateTimePicker(property);
		}
		else if (!typeof(IList).IsAssignableFrom(type))
		{
			return new TabControlTextBox(property);
		}

		return null;
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

