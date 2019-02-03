﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Atlas.Core;
using Atlas.Tabs;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Data;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlParams : Grid
	{
		private const int ControlMaxWidth = 300;
		private TabInstance tabInstance;
		private object obj;
		private bool autoGenerateRows;

		public TabControlParams(TabInstance tabInstance, object obj, bool autoGenerateRows = true)
		{
			this.tabInstance = tabInstance;
			this.obj = obj;
			this.autoGenerateRows = autoGenerateRows;

			InitializeControls();
		}

		private void InitializeControls()
		{
			//this.VerticalAlignment = VerticalAlignment.Stretch;
			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.ColumnDefinitions = new ColumnDefinitions("Auto,*");
			this.Margin = new Thickness(15, 6);
			this.MinWidth = 100;
			this.MaxWidth = 1000;

			//DataStore = (IEnumerable<object>)obj;

			if (autoGenerateRows)
			{
				ItemCollection<ListProperty> properties = ListProperty.Create(obj);
				foreach (ListProperty property in properties)
				{
					AddPropertyRow(property);
				}
			}

			//this.Focus();
		}

		public Control AddPropertyRow(PropertyInfo propertyInfo)
		{
			return AddPropertyRow(new ListProperty(obj, propertyInfo));
		}

		public Control AddPropertyRow(ListProperty property)
		{
			int rowIndex = RowDefinitions.Count;
			{
				RowDefinition spacerRow = new RowDefinition();
				spacerRow.Height = new GridLength(5);
				RowDefinitions.Add(spacerRow);
				rowIndex++;
			}
			RowDefinition gridRow = new RowDefinition()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			this.RowDefinitions.Add(gridRow);

			TextBlock textLabel = new TextBlock()
			{
				Text = property.Name,
				Margin = new Thickness(0, 3, 10, 3),
				//Margin = new Thickness(10, 0, 0, 0), // needs Padding so Border not required
				//Background = new SolidColorBrush(Theme.GridColumnHeaderBackgroundColor),
				Foreground = new SolidColorBrush(Colors.White),
				VerticalAlignment = VerticalAlignment.Center,
				//HorizontalAlignment = HorizontalAlignment.Stretch,
				MaxWidth = 500,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = 0,
			};
			this.Children.Add(textLabel);

			Type propertyType = property.propertyInfo.PropertyType;
			Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

			bool propertyReadOnly = (property.propertyInfo.GetCustomAttribute(typeof(ReadOnlyAttribute)) != null);

			BindListAttribute listAttribute = underlyingType.GetCustomAttribute(typeof(BindListAttribute)) as BindListAttribute;

			Control control = null;
			//AvaloniaObject avaloniaObject;
			if (underlyingType == typeof(bool))
			{
				control = AddCheckBox(property, rowIndex);
			}
			else if (underlyingType.IsEnum || listAttribute != null)
			{
				control = AddEnum(property, rowIndex, underlyingType, listAttribute);
			}
			else if (typeof(DateTime).IsAssignableFrom(underlyingType))
			{
				AddDateTimePicker(property, rowIndex); // has 2 controls
			}
			else
			{
				control = AddTextBox(property, rowIndex, underlyingType, propertyReadOnly);
			}
			return control;
		}

		private TextBox AddTextBox(ListProperty property, int rowIndex, Type type, bool propertyReadOnly)
		{
			TextBox textBox = new TextBox()
			{
				Background = new SolidColorBrush(Colors.White),
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				IsReadOnly = !property.Editable || propertyReadOnly,
				MinWidth = 50,
				Padding = new Thickness(6, 3),
				Focusable = true, // already set?
				MaxWidth = ControlMaxWidth,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = 1,
			};
			if (textBox.IsReadOnly)
				textBox.Background = new SolidColorBrush(Theme.TextBackgroundDisabledColor);

			PasswordCharAttribute passwordCharAttribute = property.propertyInfo.GetCustomAttribute(typeof(PasswordCharAttribute)) as PasswordCharAttribute;
			if (passwordCharAttribute != null)
				textBox.PasswordChar = passwordCharAttribute.Character;

			ExampleAttribute attribute = property.propertyInfo.GetCustomAttribute(typeof(ExampleAttribute)) as ExampleAttribute;
			if (attribute != null)
				textBox.Watermark = attribute.Text;

			var binding = new Binding(property.propertyInfo.Name)
			{
				Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Source = obj,
			};
			if (type == typeof(string) || type.IsPrimitive)
				binding.Mode = BindingMode.TwoWay;
			else
				binding.Mode = BindingMode.OneWay;
			textBox.Bind(TextBlock.TextProperty, binding);
			this.Children.Add(textBox);
			return textBox;
		}

		private CheckBox AddCheckBox(ListProperty property, int rowIndex)
		{
			CheckBox checkBox = new CheckBox()
			{
				Background = new SolidColorBrush(Colors.White),
				BorderBrush = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				BorderThickness = new Thickness(1),
				MaxWidth = 200,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = 1,
			};
			var binding = new Binding(property.propertyInfo.Name)
			{
				Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = obj,
			};
			checkBox.Bind(CheckBox.IsCheckedProperty, binding);
			this.Children.Add(checkBox);
			return checkBox;
		}

		private DropDown AddEnum(ListProperty property, int rowIndex, Type underlyingType, BindListAttribute listAttribute)
		{
			// todo: eventually handle custom lists
			//ComboBox comboBox = new ComboBox(); // AvaloniaUI doesn't implement yet :(

			DropDown dropDown = new DropDown()
			{
				Background = new SolidColorBrush(Colors.White),
				BorderBrush = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				BorderThickness = new Thickness(1),
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = 1,
			};

			if (listAttribute != null)
			{
				PropertyInfo propertyInfo = obj.GetType().GetProperty(listAttribute.Name);
				dropDown.Items = propertyInfo.GetValue(obj) as IEnumerable;
			}
			else
			{
				var values = underlyingType.GetEnumValues();
				dropDown.Items = values;
			}

			var binding = new Binding(property.propertyInfo.Name)
			{
				Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = obj,
			};
			dropDown.Bind(DropDown.SelectedItemProperty, binding);
			this.Children.Add(dropDown);
			return dropDown;
		}

		// todo: need a real DateTimePicker
		private void AddDateTimePicker(ListProperty property, int rowIndex)
		{
			DatePicker datePicker = new DatePicker()
			{
				Background = new SolidColorBrush(Colors.White),
				BorderBrush = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				BorderThickness = new Thickness(1),
				SelectedDateFormat = DatePickerFormat.Custom,
				CustomDateFormatString = "yyyy/M/d",
				Watermark = "yyyy/M/d",
				MinWidth = 100,
				MaxWidth = ControlMaxWidth,

				//MaxWidth = 200,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = 1,
			};
			var binding = new Binding(property.propertyInfo.Name)
			{
				//Converter = new FieldValueConverter(),
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = obj,
			};
			datePicker.Bind(DatePicker.SelectedDateProperty, binding);
			this.Children.Add(datePicker);

			// Add extra row for time
			RowDefinition timeRow = new RowDefinition()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			this.RowDefinitions.Add(timeRow);
			rowIndex++;

			TextBox textBox = new TextBox()
			{
				Background = new SolidColorBrush(Colors.White),
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				IsReadOnly = !property.Editable,
				Watermark = "15:30:45",
				MinWidth = 80,
				//MaxWidth = 150,
				MaxWidth = ControlMaxWidth,
				Focusable = true, // already set?
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = 1,
			};
			binding = new Binding(property.propertyInfo.Name)
			{
				Converter = new TimeValueConverter(),
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = obj,
			};
			textBox.Bind(TextBlock.TextProperty, binding);
			this.Children.Add(textBox);
		}
	}
}

