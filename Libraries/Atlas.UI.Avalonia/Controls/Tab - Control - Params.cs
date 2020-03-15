﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Windows.Input;
using Atlas.Core;
using Atlas.Extensions;
using Atlas.Resources;
using Atlas.Tabs;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Data;
using Avalonia.Collections;
using Avalonia.Input.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

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
			Children.Add(datePicker);

			// Add extra row for time
			RowDefinition timeRow = new RowDefinition()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			RowDefinitions.Add(timeRow);
			rowIndex++;

			TextBox textBox = new TabControlTextBox()
			{
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
			Children.Add(textBox);

			Button buttonImport = AddButton(rowIndex, "Import Clipboard", Icons.Streams.Paste);
			buttonImport.Click += (sender, e) =>
			{
				string clipboardText = ((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).GetTextAsync().Result;
				TimeSpan? timeSpan = ConvertTextToTimeSpan(clipboardText);
				if (timeSpan != null)
				{
					DateTime? newDateTime = dateTimeConverter.Convert(timeSpan, typeof(string), null, null) as DateTime?;
					property.propertyInfo.SetValue(property.obj, newDateTime);
					textBox.Text = timeSpan.ToString();
					e.Handled = true;
				}
				else
				{
					DateTime? dateTime = ConvertTextToDateTime(clipboardText);
					if (dateTime != null)
					{
						property.propertyInfo.SetValue(property.obj, dateTime);
						datePicker.SelectedDate = dateTime;
						textBox.Text = (string)dateTimeConverter.Convert(dateTime, typeof(string), null, null);
						e.Handled = true;
					}
				}
			};
			Children.Add(buttonImport);
		}
		private TimeSpan? ConvertTextToTimeSpan(string text)
		{
			TimeSpan timeSpan;
			if (TimeSpan.TryParseExact(text, @"h\:m\:s", CultureInfo.InvariantCulture, out timeSpan))
				return timeSpan;

			return null;
		}

		private DateTime? ConvertTextToDateTime(string text)
		{
			DateTime dateTime;

			// convert epoch 1569998557298
			var epochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			uint epochValue;
			if (text.Length == 10 && uint.TryParse(text, out epochValue))
			{
				dateTime = epochTime.AddSeconds(epochValue);
				return dateTime;
			}
			long epochValueMilliseconds;
			if (text.Length == 13 && long.TryParse(text, out epochValueMilliseconds))
			{
				dateTime = epochTime.AddMilliseconds(epochValueMilliseconds);
				return dateTime;
			}

			if (DateTime.TryParse(text, out dateTime)
				//|| DateTime.TryParseExact(text, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime) // July 25 05:08:00
				|| DateTime.TryParseExact(text, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime) // 18/Jul/2019:11:47:45 +0000
				|| DateTime.TryParseExact(text, "dd/MMM/yyyy:HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime)) // 18/Jul/2019:11:47:45
			{
				if (dateTime.Kind == DateTimeKind.Unspecified)
					dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
				else if (dateTime.Kind == DateTimeKind.Local)
					dateTime = dateTime.ToUniversalTime();
				return dateTime;
			}

			return null;
		}

		public Button AddButton(int rowIndex, string tooltip, Stream resource, ICommand command = null)
		{
			//command = command ?? new RelayCommand(
			//	(obj) => CommandDefaultCanExecute(obj),
			//	(obj) => CommandDefaultExecute(obj));
			Bitmap bitmap;
			using (resource)
			{
				bitmap = new Bitmap(resource);
			}

			var image = new Image()
			{
				Source = bitmap,
				Width = 16,
				Height = 16,
			};

			Button button = new Button()
			{
				Content = image,
				Command = command,
				Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor),
				BorderBrush = Background,
				BorderThickness = new Thickness(0),
				//Margin = new Thickness(2),
				HorizontalAlignment = HorizontalAlignment.Right,
				//BorderThickness = new Thickness(2),
				//Foreground = new SolidColorBrush(Theme.ButtonForegroundColor),
				//BorderBrush = new SolidColorBrush(Colors.Black),

				[ToolTip.TipProperty] = tooltip,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = 1,
			};
			button.BorderBrush = button.Background;
			button.PointerEnter += Button_PointerEnter;
			button.PointerLeave += Button_PointerLeave;

			//var button = new ToolbarButton(tooltip, command, resource);
			//AddControl(button);
			return button;
		}

		// DefaultTheme.xaml is overriding this currently
		private void Button_PointerEnter(object sender, PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			button.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundHoverColor);
		}

		private void Button_PointerLeave(object sender, PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor);
			button.BorderBrush = button.Background;
		}
	}
}
