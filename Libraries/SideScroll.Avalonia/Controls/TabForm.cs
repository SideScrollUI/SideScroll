using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls.View;
using SideScroll.Avalonia.Utilities;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SideScroll.Avalonia.Controls;

/// <summary>A section header border used to visually group properties in a <see cref="TabForm"/>.</summary>
public class TabHeader : Border
{
	public TabHeader(string text)
	{
		Child = new TextBlock
		{
			Text = text,
		};
	}
}

/// <summary>A visual horizontal separator used between property rows in a <see cref="TabForm"/>.</summary>
public class TabSeparator : Border;

/// <summary>
/// A property editor that uses reflection to generate input controls (text boxes, combo boxes, check boxes, etc.)
/// for each public, visible property of a bound object, supporting validation, grouping, and data binding.
/// </summary>
public class TabForm : Border, IValidationControl
{
	/// <summary>Gets or sets the maximum width in pixels applied to form input controls.</summary>
	public static int ControlMaxWidth { get; set; } = 2000;

	/// <summary>Gets or sets the maximum height in pixels applied to form input controls.</summary>
	public static int ControlMaxHeight { get; set; } = 400;

	/// <summary>Gets or sets the object whose properties are displayed in this form.</summary>
	public object? Object { get; set; }

	/// <summary>Gets the form object configuration, or <c>null</c> if constructed directly.</summary>
	public TabFormObject? FormObject { get; }

	/// <summary>Gets the container grid that holds all the property label/control rows.</summary>
	public Grid ContainerGrid { get; protected set; }

	private readonly Dictionary<ListProperty, Control> _propertyControls = [];

	/// <summary>Returns the string representation of the bound object.</summary>
	public override string? ToString() => Object?.ToString();

	public TabForm(object? obj, bool autoGenerateRows = true, string columnDefinitions = "Auto,*")
	{
		Object = obj;

		InitializeControls(columnDefinitions);

		if (autoGenerateRows)
		{
			LoadObject(obj);
		}
	}

	public TabForm(TabFormObject formObject, bool autoGenerateRows = true) : this(formObject.Object, autoGenerateRows)
	{
		FormObject = formObject;
		FormObject.ObjectChanged += FormObject_ObjectChanged;
		FormObject.OnFocus += FormObject_OnFocus;
	}

	private void FormObject_ObjectChanged(object? sender, ObjectUpdatedEventArgs e)
	{
		LoadObject(e.Object);
	}

	private void FormObject_OnFocus(object? sender, EventArgs e)
	{
		Focus();
	}

	[MemberNotNull(nameof(ContainerGrid))]
	private void InitializeControls(string columnDefinitions)
	{
		ContainerGrid = new()
		{
			ColumnDefinitions = new ColumnDefinitions(columnDefinitions),
			RowDefinitions = new RowDefinitions("*"),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			Margin = new Thickness(6, 6, 10, 6), // Extra for ScrollBar on right side
		};

		Child = ContainerGrid;

		MinWidth = 100;
		MaxWidth = 2000;
	}

	private void ClearControls()
	{
		ContainerGrid.Children.Clear();
		ContainerGrid.RowDefinitions.Clear();
		_propertyControls.Clear();
	}

	/// <summary>Clears the existing form controls and regenerates rows for all visible properties of <paramref name="obj"/>.</summary>
	public void LoadObject(object? obj)
	{
		ClearControls();

		Object = obj;
		if (obj == null) return;

		AddPropertyControls(obj);
	}

	private void AddPropertyControls(object obj)
	{
		ItemCollection<ListProperty> properties = ListProperty.Create(obj, includeStatic: false);

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
				if (lastControl != null && Grid.GetRow(lastControl) != Grid.GetRow(newControl))
				{
					FillColumnSpan(lastControl);
				}
				_propertyControls[property] = newControl;
				lastControl = newControl;
			}
		}

		if (lastControl != null)
		{
			FillColumnSpan(lastControl);
		}
	}

	// Fill entire last line if available
	private void FillColumnSpan(Control lastControl)
	{
		int columnIndex = Grid.GetColumn(lastControl);
		int columnSpan = Grid.GetColumnSpan(lastControl);
		if (columnIndex + columnSpan < ContainerGrid.ColumnDefinitions.Count)
		{
			Grid.SetColumnSpan(lastControl, ContainerGrid.ColumnDefinitions.Count - columnIndex);
		}
	}

	/// <summary>Adds a row of property controls for each visible property of <paramref name="obj"/> and returns the created controls.</summary>
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
		int rowIndex = ContainerGrid.RowDefinitions.Count;
		RowDefinition rowDefinition = new()
		{
			Height = new GridLength(1, GridUnitType.Auto),
		};
		ContainerGrid.RowDefinitions.Add(rowDefinition);
		return rowIndex;
	}

	private void AddControl(Control control, int columnIndex, int rowIndex)
	{
		AddColumnIndex(columnIndex);

		Grid.SetColumn(control, columnIndex);
		Grid.SetRow(control, rowIndex);
		ContainerGrid.Children.Add(control);
	}

	private void AddColumnIndex(int columnIndex)
	{
		while (columnIndex >= ContainerGrid.ColumnDefinitions.Count)
		{
			GridUnitType type = (ContainerGrid.ColumnDefinitions.Count % 2 == 0) ? GridUnitType.Auto : GridUnitType.Star;
			var columnDefinition = new ColumnDefinition(1, type);
			ContainerGrid.ColumnDefinitions.Add(columnDefinition);
		}
	}

	/// <summary>Adds a label-and-input row for the named property on the bound object.</summary>
	public Control? AddPropertyControl(string propertyName)
	{
		PropertyInfo propertyInfo = Object!.GetType().GetProperty(propertyName)!;
		return AddPropertyControl(new ListProperty(Object, propertyInfo));
	}

	/// <summary>Adds a label-and-input row for the given <see cref="PropertyInfo"/> on the bound object.</summary>
	public Control? AddPropertyControl(PropertyInfo propertyInfo)
	{
		return AddPropertyControl(new ListProperty(Object!, propertyInfo));
	}

	/// <summary>Adds a label-and-input row for the given property, reusing the current row when a column index attribute is specified.</summary>
	public Control? AddPropertyControl(ListProperty property)
	{
		Control? control = CreatePropertyControl(property);
		if (control == null)
			return null;

		property.IsCacheable = false;

		int columnIndex = property.GetCustomAttribute<ColumnIndexAttribute>()?.Index ?? 0;
		int rowIndex = ContainerGrid.RowDefinitions.Count;

		if (rowIndex > 0 && columnIndex > 0 &&
			ContainerGrid.Children.LastOrDefault() is Control lastControl && Grid.GetColumn(lastControl) < columnIndex)
		{
			rowIndex--; // Reuse previous row
		}
		else
		{
			AddSpacer();
			rowIndex++;

			RowDefinition rowDefinition = new()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			ContainerGrid.RowDefinitions.Add(rowDefinition);
		}

		TabTextBlock textLabel = new()
		{
			Text = property.Name,
			Margin = new Thickness(10, 7, 10, 3),
			VerticalAlignment = VerticalAlignment.Top,
			MaxWidth = ControlMaxWidth,
			[Grid.RowProperty] = rowIndex,
			[Grid.ColumnProperty] = columnIndex++,
		};
		ContainerGrid.Children.Add(textLabel);

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
		ContainerGrid.RowDefinitions.Add(spacerRow);
	}

	private static Control? CreatePropertyControl(ListProperty property)
	{
		Type type = property.UnderlyingType;

		BindListAttribute? listAttribute = type.GetCustomAttribute<BindListAttribute>();
		listAttribute ??= property.GetCustomAttribute<BindListAttribute>();

		if (property.IsEditable)
		{
			if (type.IsEnum || listAttribute != null)
			{
				return new TabFormattedComboBox(property, listAttribute?.PropertyName);
			}
			else if (typeof(DateTime).IsAssignableFrom(type))
			{
				return new TabDateTimePicker(property);
			}
		}

		if (type == typeof(bool))
		{
			return new TabCheckBox(property);
		}
		else if (typeof(Color).IsAssignableFrom(type))
		{
			return new TabColorPicker(property);
		}
		else if (typeof(string).IsAssignableFrom(type) || !typeof(IEnumerable).IsAssignableFrom(type))
		{
			return new TabTextBox(property);
		}

		return null;
	}

	/// <summary>Appends a full-width header label spanning all columns to the form.</summary>
	public void AddHeader(string text)
	{
		if (ContainerGrid.Children.Count > 0)
		{
			AddSpacer(10);
		}

		TabHeader header = new(text)
		{
			[Grid.ColumnSpanProperty] = ContainerGrid.ColumnDefinitions.Count,
		};

		int rowIndex = ContainerGrid.RowDefinitions.Count;
		AddRowDefinition();
		AddControl(header, 0, rowIndex);
	}

	/// <summary>Appends a full-width visual separator row to the form, unless the form is empty.</summary>
	public void AddSeparator()
	{
		// Don't add for first item or optional null controls
		if (ContainerGrid.Children.Count == 0) return;

		TabSeparator separator = new()
		{
			[Grid.ColumnSpanProperty] = ContainerGrid.ColumnDefinitions.Count,
		};

		int rowIndex = ContainerGrid.RowDefinitions.Count;
		AddRowDefinition();
		AddControl(separator, 0, rowIndex);
	}

	/// <summary>Focuses the first text-box input control in the form, or the form itself if none is found.</summary>
	public void Focus()
	{
		foreach (Control control in ContainerGrid.Children)
		{
			if (control is TextBox textBox)
			{
				textBox.Focus();
				return;
			}
		}
		base.Focus();
	}

	/// <summary>Validates all bound property controls and highlights any that fail validation.</summary>
	public void Validate()
	{
		bool valid = true;
		foreach (var propertyControl in _propertyControls)
		{
			if (!AvaloniaUtils.ValidateControl(propertyControl.Key, propertyControl.Value))
			{
				if (valid)
				{
					propertyControl.Value.Focus();
					valid = false;
				}
			}
		}

		if (!valid)
		{
			throw new ValidationException("Invalid Values");
		}
	}
}
