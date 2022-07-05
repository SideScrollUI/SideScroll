using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlParams : Grid
{
	public const int ControlMaxWidth = 500;
	public object Object;

	public TabControlParams(object obj, bool autoGenerateRows = true, string columnDefinitions = "Auto,*")
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

		Margin = new Thickness(15, 6);

		MinWidth = 100;
		MaxWidth = 2000;
	}

	private void ClearControls()
	{
		Children.Clear();
		RowDefinitions.Clear();
	}

	public void LoadObject(object obj)
	{
		ClearControls();

		AddSummary();

		ItemCollection<ListProperty> properties = ListProperty.Create(obj);
		foreach (ListProperty property in properties)
		{
			AddPropertyRow(property);
		}
	}

	private void AddSummary()
	{
		var summaryAttribute = Object.GetType().GetCustomAttribute<SummaryAttribute>();
		if (summaryAttribute == null)
			return;

		AddRowDefinition();

		var textBlock = new TextBlock()
		{
			Text = summaryAttribute.Summary,
			FontSize = 14,
			Margin = new Thickness(0, 3, 10, 3),
			Foreground = Theme.BackgroundText,
			VerticalAlignment = VerticalAlignment.Top,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			TextWrapping = TextWrapping.Wrap,
			MaxWidth = 500,
			[Grid.ColumnSpanProperty] = 2,
		};
		Children.Add(textBlock);
	}

	public List<Control> AddObjectRow(object obj, List<PropertyInfo> properties = null)
	{
		properties ??= obj.GetType().GetVisibleProperties();

		int rowIndex = AddRowDefinition();
		int columnIndex = 0;

		var controls = new List<Control>();
		foreach (PropertyInfo propertyInfo in properties)
		{
			var property = new ListProperty(obj, propertyInfo);
			Control control = CreatePropertyControl(property);
			if (control == null)
				continue;

			AddControl(control, columnIndex, rowIndex);
			controls.Add(control);
			columnIndex++;
		}
		return controls;
	}

	private int AddRowDefinition()
	{
		int rowIndex = RowDefinitions.Count;
		var rowDefinition = new RowDefinition()
		{
			Height = new GridLength(1, GridUnitType.Auto),
		};
		RowDefinitions.Add(rowDefinition);
		return rowIndex;
	}

	private void AddControl(Control control, int columnIndex, int rowIndex)
	{
		SetColumn(control, columnIndex);
		SetRow(control, rowIndex);
		Children.Add(control);
	}

	public Control AddPropertyRow(string propertyName)
	{
		PropertyInfo propertyInfo = Object.GetType().GetProperty(propertyName);
		return AddPropertyRow(new ListProperty(Object, propertyInfo));
	}

	public Control AddPropertyRow(PropertyInfo propertyInfo)
	{
		return AddPropertyRow(new ListProperty(Object, propertyInfo));
	}

	public Control AddPropertyRow(ListProperty property)
	{
		Control control = CreatePropertyControl(property);
		if (control == null)
			return null;

		int rowIndex = RowDefinitions.Count;
		{
			var spacerRow = new RowDefinition()
			{
				Height = new GridLength(5),
			};
			RowDefinitions.Add(spacerRow);
			rowIndex++;
		}
		var rowDefinition = new RowDefinition()
		{
			Height = new GridLength(1, GridUnitType.Auto),
		};
		RowDefinitions.Add(rowDefinition);

		var textLabel = new TextBlock()
		{
			Text = property.Name,
			Margin = new Thickness(0, 3, 10, 3),
			Foreground = Theme.BackgroundText,
			VerticalAlignment = VerticalAlignment.Center,
			//HorizontalAlignment = HorizontalAlignment.Stretch,
			MaxWidth = 500,
			[Grid.RowProperty] = rowIndex,
			[Grid.ColumnProperty] = 0,
		};
		Children.Add(textLabel);

		AddControl(control, 1, rowIndex);

		return control;
	}

	private Control CreatePropertyControl(ListProperty property)
	{
		Type type = property.UnderlyingType;

		BindListAttribute listAttribute = type.GetCustomAttribute<BindListAttribute>();
		listAttribute ??= property.PropertyInfo.GetCustomAttribute<BindListAttribute>();

		Control control = null;
		if (type == typeof(bool))
		{
			control = new TabControlCheckBox(property);
		}
		else if (type.IsEnum || listAttribute != null)
		{
			control = new TabControlComboBox(property, listAttribute?.PropertyName);
		}
		else if (typeof(DateTime).IsAssignableFrom(type))
		{
			control = new TabDateTimePicker(property);
		}
		else if (!typeof(IList).IsAssignableFrom(type))
		{
			control = new TabControlTextBox(property);
		}

		return control;
	}

	// Focus first input control
	// Add [Focus] attribute if more control needed?
	public new void Focus()
	{
		foreach (IControl control in Children)
		{
			if (control is TextBox textBox)
			{
				textBox.Focus();
				return;
			}
		}
		base.Focus();
	}
}

