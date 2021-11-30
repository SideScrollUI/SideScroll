using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
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

		public readonly ListProperty Property;

		public Binding Binding { get; set; }

		private DateTimeValueConverter _dateTimeConverter;
		private CalendarDatePicker _datePicker;
		private TabControlTextBox _timeTextBox;

		public TabDateTimePicker(ListProperty property)
		{
			Property = property;

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			ColumnDefinitions = new ColumnDefinitions("*,*,Auto");
			RowDefinitions = new RowDefinitions("Auto");

			_dateTimeConverter = new DateTimeValueConverter();

			Binding = new Binding(Property.PropertyInfo.Name)
			{
				Converter = _dateTimeConverter,
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = Property.Object,
			};

			AddDatePicker();
			AddTimeTextBox();

			Button buttonImport = AddButton("Import Clipboard", Icons.Streams.Paste);
			buttonImport.Click += ButtonImport_Click;
			Children.Add(buttonImport);
		}

		private void AddDatePicker()
		{
			_datePicker = new CalendarDatePicker()
			{
				Background = Property.Editable ? Theme.Background : Brushes.LightGray,
				BorderBrush = Brushes.Black,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				BorderThickness = new Thickness(1),
				SelectedDateFormat = CalendarDatePickerFormat.Custom,
				CustomDateFormatString = "yyyy/M/d",
				Watermark = "yyyy/M/d",
				MinWidth = 105,
				MaxWidth = 300,
				IsEnabled = Property.Editable,
			};

			_datePicker.Bind(CalendarDatePicker.SelectedDateProperty, Binding);
			Children.Add(_datePicker);
		}

		private void AddTimeTextBox()
		{
			_timeTextBox = new TabControlTextBox()
			{
				IsReadOnly = !Property.Editable,
				Watermark = "15:30:45",
				Margin = new Thickness(8, 0, 0, 0),
				MinWidth = 75,
				MaxWidth = 300,
				Focusable = true, // already set?
				[Grid.ColumnProperty] = 1,
			};

			if (!Property.Editable)
				_timeTextBox.Background = Theme.TextBackgroundDisabled;

			_timeTextBox.Bind(TextBlock.TextProperty, Binding);
			Children.Add(_timeTextBox);
		}

		private void ButtonImport_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			string clipboardText = ClipBoardUtils.GetTextAsync().Result;
			TimeSpan? timeSpan = DateTimeUtils.ConvertTextToTimeSpan(clipboardText);
			if (timeSpan != null)
			{
				DateTime? newDateTime = _dateTimeConverter.Convert(timeSpan, typeof(string), null, null) as DateTime?;
				Property.PropertyInfo.SetValue(Property.Object, newDateTime);
				_timeTextBox.Text = timeSpan.ToString();
				e.Handled = true;
			}
			else
			{
				DateTime? dateTime = DateTimeUtils.ConvertTextToDateTime(clipboardText);
				if (dateTime != null)
				{
					Property.PropertyInfo.SetValue(Property.Object, dateTime);
					_datePicker.SelectedDate = dateTime;
					_timeTextBox.Text = (string)_dateTimeConverter.Convert(dateTime, typeof(string), null, null);
					e.Handled = true;
				}
			}
		}

		public Button AddButton(string tooltip, Stream resource)
		{
			//command ??= new RelayCommand(
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
