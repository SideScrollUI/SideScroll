using System;
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
using Atlas.Extensions;
using Avalonia.Collections;
using Avalonia.Input.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlParams : Grid
	{
		private const int ControlMaxWidth = 400;
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
			this.MaxWidth = 2000;

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
			this.RowDefinitions.Add(gridRow);

			List<Control> controls = new List<Control>();
			foreach (PropertyInfo propertyInfo in obj.GetType().GetVisibleProperties())
			{
				var property = new ListProperty(obj, propertyInfo);
				Control control = AddProperty(property, rowIndex, columnIndex);
				controls.Add(control);
				columnIndex++;
			}
			rowIndex++;
			return controls;
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
			Control control = AddProperty(property, rowIndex, 1);
			rowIndex++;
			return control;
		}

		private Control AddProperty(ListProperty property, int rowIndex, int columnIndex)
		{
			Type propertyType = property.propertyInfo.PropertyType;
			Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

			BindListAttribute listAttribute = underlyingType.GetCustomAttribute<BindListAttribute>();

			Control control = null;
			//AvaloniaObject avaloniaObject;
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
			TextBox textBox = new TextBox()
			{
				Background = new SolidColorBrush(Colors.White),
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				IsReadOnly = !property.Editable,
				MinWidth = 50,
				Padding = new Thickness(6, 3),
				Focusable = true, // already set?
				MaxWidth = ControlMaxWidth,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = columnIndex,
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
			AddTextBoxContextMenu(textBox);

			this.Children.Add(textBox);
			return textBox;
		}

		private void SendTextBoxKey(TextBox textBox, List<KeyGesture> keyGestures)
		{
			foreach (var key in keyGestures)
			{
				var args = new KeyEventArgs()
				{
					Key = key.Key,
					Modifiers = key.Modifiers,
					RoutedEvent = TextBox.KeyDownEvent,
				};

				textBox.RaiseEvent(args);
				break;
			}
		}

		private void AddTextBoxContextMenu(TextBox textBox)
		{
			ContextMenu contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();
			MenuItem menuItemCut = new MenuItem() { Header = "Cut" };
			menuItemCut.Click += delegate { SendTextBoxKey(textBox, keymap.Cut); };
			list.Add(menuItemCut);

			MenuItem menuItemCopy = new MenuItem() { Header = "_Copy" };
			menuItemCopy.Click += delegate { SendTextBoxKey(textBox, keymap.Copy); };
			list.Add(menuItemCopy);

			MenuItem menuItemPaste = new MenuItem() { Header = "Paste" };
			menuItemPaste.Click += delegate { SendTextBoxKey(textBox, keymap.Paste); };
			list.Add(menuItemPaste);

			//list.Add(new Separator());

			contextMenu.Items = list;

			textBox.ContextMenu = contextMenu;
		}

		private CheckBox AddCheckBox(ListProperty property, int rowIndex, int columnIndex)
		{
			CheckBox checkBox = new CheckBox()
			{
				Background = new SolidColorBrush(Colors.White),
				BorderBrush = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				BorderThickness = new Thickness(1),
				MaxWidth = ControlMaxWidth,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = columnIndex,
			};
			var binding = new Binding(property.propertyInfo.Name)
			{
				Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = property.obj,
			};
			checkBox.Bind(CheckBox.IsCheckedProperty, binding);
			this.Children.Add(checkBox);
			return checkBox;
		}

		private DropDown AddEnum(ListProperty property, int rowIndex, int columnIndex, Type underlyingType, BindListAttribute propertyListAttribute)
		{
			// todo: eventually handle custom lists
			//ComboBox comboBox = new ComboBox(); // AvaloniaUI doesn't implement yet :(

			DropDown dropDown = new DropDown()
			{
				Background = new SolidColorBrush(Colors.White),
				BorderBrush = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				BorderThickness = new Thickness(1),
				MaxWidth = ControlMaxWidth,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = columnIndex,
			};

			if (propertyListAttribute != null)
			{
				PropertyInfo propertyInfo = property.obj.GetType().GetProperty(propertyListAttribute.Name);
				dropDown.Items = propertyInfo.GetValue(property.obj) as IEnumerable;
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
				Source = property.obj,
			};
			dropDown.Bind(DropDown.SelectedItemProperty, binding);
			this.Children.Add(dropDown);
			return dropDown;
		}

		// todo: need a real DateTimePicker
		private void AddDateTimePicker(ListProperty property, int rowIndex, int columnIndex)
		{
			var backgroundColor = property.Editable ? Colors.White : Colors.LightGray;
			DatePicker datePicker = new DatePicker()
			{
				Background = new SolidColorBrush(backgroundColor),
				BorderBrush = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				BorderThickness = new Thickness(1),
				SelectedDateFormat = DatePickerFormat.Custom,
				CustomDateFormatString = "yyyy/M/d",
				Watermark = "yyyy/M/d",
				MinWidth = 100,
				MaxWidth = ControlMaxWidth,
				IsEnabled = property.Editable,

				//MaxWidth = 200,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = columnIndex,
			};
			var dateTimeConverter = new DateTimeValueConverter();
			var binding = new Binding(property.propertyInfo.Name)
			{
				Converter = dateTimeConverter,
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = property.obj,
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
				Background = new SolidColorBrush(backgroundColor),
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
				[Grid.ColumnProperty] = columnIndex,
			};
			binding = new Binding(property.propertyInfo.Name)
			{
				Converter = dateTimeConverter,
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = property.obj,
			};
			textBox.Bind(TextBlock.TextProperty, binding);
			this.Children.Add(textBox);
		}
	}
}

