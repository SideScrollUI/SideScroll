using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media.Imaging;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlParams : Grid
	{
		public const int ControlMaxWidth = 500;
		private TabInstance tabInstance;
		private object obj;
		private bool autoGenerateRows;

		public TabControlParams(TabInstance tabInstance, object obj, bool autoGenerateRows = true, string columnDefinitions = "Auto,*")
		{
			this.tabInstance = tabInstance;
			this.obj = obj;
			this.autoGenerateRows = autoGenerateRows;

			InitializeControls(columnDefinitions);
		}

		private void InitializeControls(string columnDefinitions)
		{
			//VerticalAlignment = VerticalAlignment.Stretch;
			HorizontalAlignment = HorizontalAlignment.Stretch;
			ColumnDefinitions = new ColumnDefinitions(columnDefinitions);
			Margin = new Thickness(15, 6);
			MinWidth = 100;
			MaxWidth = 2000;

			if (autoGenerateRows)
			{
				ItemCollection<ListProperty> properties = ListProperty.Create(obj);
				foreach (ListProperty property in properties)
				{
					AddPropertyRow(property);
				}
			}
		}

		public List<Control> AddObjectRow(object obj)
		{
			int rowIndex = RowDefinitions.Count;
			int columnIndex = 0;

			/*RowDefinition spacerRow = new RowDefinition();
			spacerRow.Height = new GridLength(5);
			RowDefinitions.Add(spacerRow);*/

			RowDefinition gridRow = new RowDefinition()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			RowDefinitions.Add(gridRow);

			List<Control> controls = new List<Control>();
			foreach (PropertyInfo propertyInfo in obj.GetType().GetVisibleProperties())
			{
				var property = new ListProperty(obj, propertyInfo);
				Control control = AddProperty(property, rowIndex, columnIndex);
				controls.Add(control);
				columnIndex++;
			}
			return controls;
		}

		public Control AddPropertyRow(string propertyName)
		{
			PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
			return AddPropertyRow(new ListProperty(obj, propertyInfo));
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
			RowDefinitions.Add(gridRow);

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
			Children.Add(textLabel);
			Control control = AddProperty(property, rowIndex, 1);
			return control;
		}

		private Control AddProperty(ListProperty property, int rowIndex, int columnIndex)
		{
			Type propertyType = property.propertyInfo.PropertyType;
			Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

			BindListAttribute listAttribute = underlyingType.GetCustomAttribute<BindListAttribute>();

			Control control = null;
			if (underlyingType == typeof(bool))
			{
				control = AddCheckBox(property, rowIndex, columnIndex);
			}
			else if (underlyingType.IsEnum || listAttribute != null)
			{
				control = AddEnum(property, rowIndex, columnIndex, underlyingType, listAttribute);
			}
			else if (typeof(DateTime).IsAssignableFrom(underlyingType))
			{
				AddDateTimePicker(property, rowIndex, columnIndex); // has 2 controls
			}
			else
			{
				control = AddTextBox(property, rowIndex, columnIndex, underlyingType);
			}

			return control;
		}

		private TextBox AddTextBox(ListProperty property, int rowIndex, int columnIndex, Type type)
		{
			var textBox = new TabControlTextBox()
			{
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = columnIndex,
				IsReadOnly = !property.Editable,
			};
			if (textBox.IsReadOnly)
				textBox.Background = new SolidColorBrush(Theme.TextBackgroundDisabledColor);

			PasswordCharAttribute passwordCharAttribute = property.propertyInfo.GetCustomAttribute<PasswordCharAttribute>();
			if (passwordCharAttribute != null)
				textBox.PasswordChar = passwordCharAttribute.Character;

			ExampleAttribute attribute = property.propertyInfo.GetCustomAttribute<ExampleAttribute>();
			if (attribute != null)
				textBox.Watermark = attribute.Text;

			var binding = new Binding(property.propertyInfo.Name)
			{
				Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Source = property.obj,
			};
			if (type == typeof(string) || type.IsPrimitive)
				binding.Mode = BindingMode.TwoWay;
			else
				binding.Mode = BindingMode.OneWay;
			textBox.Bind(TextBlock.TextProperty, binding);
			AvaloniaUtils.AddTextBoxContextMenu(textBox);

			Children.Add(textBox);
			return textBox;
		}
		private CheckBox AddCheckBox(ListProperty property, int rowIndex, int columnIndex)
		{
			TabControlCheckBox checkBox = new TabControlCheckBox(property)
			{
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = columnIndex,
			};
			Children.Add(checkBox);
			return checkBox;
		}

		private ComboBox AddEnum(ListProperty property, int rowIndex, int columnIndex, Type underlyingType, BindListAttribute propertyListAttribute)
		{
			// todo: handle custom lists
			ComboBox comboBox = new TabControlComboBox()
			{
				MaxWidth = ControlMaxWidth,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = columnIndex,
			};

			if (propertyListAttribute != null)
			{
				PropertyInfo propertyInfo = property.obj.GetType().GetProperty(propertyListAttribute.Name);
				comboBox.Items = propertyInfo.GetValue(property.obj) as IEnumerable;
			}
			else
			{
				var values = underlyingType.GetEnumValues();
				comboBox.Items = values;
			}

			var binding = new Binding(property.propertyInfo.Name)
			{
				Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = property.obj,
			};
			comboBox.Bind(ComboBox.SelectedItemProperty, binding);
			Children.Add(comboBox);
			return comboBox;
		}

		// todo: need a real DateTimePicker
		private TabDateTimePicker AddDateTimePicker(ListProperty property, int rowIndex, int columnIndex)
		{
			TabDateTimePicker control = new TabDateTimePicker(property)
			{
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = columnIndex,
			};
			Children.Add(control);
			return control;
		}
	}
}

