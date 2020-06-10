using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using System;
using System.IO;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabDateTimePicker : Grid, IStyleable
	{
		Type IStyleable.StyleKey => typeof(TabDateTimePicker);

		public TabDateTimePicker(ListProperty property)
		{
			InitializeComponent(property);
		}

		private void InitializeComponent(ListProperty property)
		{
			ColumnDefinitions = new ColumnDefinitions("*,*");
			var backgroundColor = property.Editable ? Theme.Background : Brushes.LightGray;
			var datePicker = new DatePicker()
			{
				Background = backgroundColor,
				BorderBrush = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				BorderThickness = new Thickness(1),
				SelectedDateFormat = DatePickerFormat.Custom,
				CustomDateFormatString = "yyyy/M/d",
				Watermark = "yyyy/M/d",
				MinWidth = 90,
				MaxWidth = 300,
				IsEnabled = property.Editable,

				//MaxWidth = 200,
				[Grid.ColumnProperty] = 0,
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

			var textBox = new TabControlTextBox()
			{
				IsReadOnly = !property.Editable,
				Watermark = "15:30:45",
				Margin = new Thickness(6, 0, 0, 0),
				MinWidth = 75,
				//MaxWidth = 150,
				MaxWidth = 300,
				Focusable = true, // already set?
				//[Grid.RowProperty] = 0,
				[Grid.ColumnProperty] = 1,
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

			Button buttonImport = AddButton("Import Clipboard", Icons.Streams.Paste);
			buttonImport.Click += (sender, e) =>
			{
				string clipboardText = ((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).GetTextAsync().Result;
				TimeSpan? timeSpan = DateTimeUtils.ConvertTextToTimeSpan(clipboardText);
				if (timeSpan != null)
				{
					DateTime? newDateTime = dateTimeConverter.Convert(timeSpan, typeof(string), null, null) as DateTime?;
					property.propertyInfo.SetValue(property.obj, newDateTime);
					textBox.Text = timeSpan.ToString();
					e.Handled = true;
				}
				else
				{
					DateTime? dateTime = DateTimeUtils.ConvertTextToDateTime(clipboardText);
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

		public Button AddButton(string tooltip, Stream resource)
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

			var button = new Button()
			{
				Content = image,
				//Command = command,
				//Background = Brushes.Transparent,
				Background = Theme.TabBackground,
				BorderBrush = Background,
				BorderThickness = new Thickness(0),
				//Margin = new Thickness(2),
				HorizontalAlignment = HorizontalAlignment.Right,
				//BorderThickness = new Thickness(2),
				//Foreground = new SolidColorBrush(Theme.ButtonForegroundColor),
				//BorderBrush = new SolidColorBrush(Colors.Black),

				[ToolTip.TipProperty] = tooltip,
				[Grid.ColumnProperty] = 2,
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
			button.Background = Theme.ToolbarButtonBackgroundHover;
		}

		private void Button_PointerLeave(object sender, PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = Theme.TabBackground;
			//button.Background = Brushes.Transparent;
			button.BorderBrush = button.Background;
		}
	}
}
