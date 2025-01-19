using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SideScroll.Utilities;
using SideScroll.Avalonia.Controls.Converters;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SideScroll.Avalonia.Controls;

public class TabCalendarDatePicker : CalendarDatePicker
{
	protected override Type StyleKeyOverride => typeof(CalendarDatePicker);

	// Default behavior increments and decrements Date when scrolling left/right with the mousepad
	// This is probably useful for Mobile devices, but not Desktop
	// Override and do nothing instead
	protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
	{
	}
}

public class TabDateTimePicker : Grid
{
	public ListProperty Property { get; set; }

	public Binding Binding { get; set; }

	private readonly DateTimeValueConverter _dateTimeConverter;
	private TabCalendarDatePicker _datePicker;
	private TabControlTextBox _timeTextBox;

	public TabDateTimePicker(ListProperty property)
	{
		Property = property;

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

		if (Property.Editable)
		{
			Button buttonImport = AddButton("Import Clipboard", Icons.Png.Paste.Stream);
			buttonImport.Click += ButtonImport_Click;
			Children.Add(buttonImport);
		}
	}

	[MemberNotNull(nameof(_datePicker))]
	private void AddDatePicker()
	{
		_datePicker = new TabCalendarDatePicker
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Top, // Validation errors appear below controls
			VerticalContentAlignment = VerticalAlignment.Center,
			SelectedDateFormat = CalendarDatePickerFormat.Custom,
			CustomDateFormatString = "yyyy/M/d",
			Watermark = "yyyy/M/d",
			MinWidth = 105,
			MaxWidth = TabControlParams.ControlMaxWidth,
			IsEnabled = Property.Editable,
		};

		if (!Property.Editable)
		{
			_datePicker.Background = SideScrollTheme.TextReadOnlyBackground;
		}

		_datePicker.Bind(CalendarDatePicker.SelectedDateProperty, Binding);
		Children.Add(_datePicker);
	}

	[MemberNotNull(nameof(_timeTextBox))]
	private void AddTimeTextBox()
	{
		_timeTextBox = new TabControlTextBox
		{
			IsReadOnly = !Property.Editable,
			Watermark = "15:30:45",
			Margin = new Thickness(10, 0, 0, 0),
			MinWidth = 75,
			MaxWidth = TabControlParams.ControlMaxWidth,
			Focusable = true, // already set?
			[Grid.ColumnProperty] = 1,
		};

		if (!Property.Editable)
		{
			_timeTextBox.Background = SideScrollTheme.TextReadOnlyBackground;
			_timeTextBox.Foreground = SideScrollTheme.TextReadOnlyForeground;
		}

		_timeTextBox.Bind(TextBlock.TextProperty, Binding);
		Children.Add(_timeTextBox);
	}

	private void ButtonImport_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		string? clipboardText = ClipboardUtils.GetText(this);
		if (clipboardText == null) return;

		if (DateTimeUtils.TryParseTimeSpan(clipboardText, out TimeSpan timeSpan))
		{
			DateTime? newDateTime = _dateTimeConverter.Convert(timeSpan, typeof(string), null, CultureInfo.InvariantCulture) as DateTime?;
			Property.PropertyInfo.SetValue(Property.Object, newDateTime);
			_timeTextBox.Text = timeSpan.ToString();
			e.Handled = true;
		}
		else
		{
			if (DateTimeUtils.TryParseDateTime(clipboardText, out DateTime dateTime))
			{
				Property.PropertyInfo.SetValue(Property.Object, dateTime);
				_datePicker.SelectedDate = dateTime;
				_timeTextBox.Text = (string)_dateTimeConverter.Convert(dateTime, typeof(string), null, CultureInfo.InvariantCulture)!;
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

		var image = new Image
		{
			Source = bitmap,
			Width = 16,
			Height = 16,
		};

		var button = new Button
		{
			Content = image,
			//Command = command,
			//Background = Brushes.Transparent,
			Background = SideScrollTheme.TabBackground,
			BorderBrush = Background,
			BorderThickness = new Thickness(0),
			//Margin = new Thickness(2),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,

			[ToolTip.TipProperty] = tooltip,
			[Grid.ColumnProperty] = 2,
		};
		button.BorderBrush = button.Background;
		button.PointerEntered += Button_PointerEnter;
		button.PointerExited += Button_PointerExited;

		//var button = new ToolbarButton(tooltip, command, resource);
		//AddControl(button);
		return button;
	}

	// DefaultTheme.xaml is overriding this currently
	private static void Button_PointerEnter(object? sender, PointerEventArgs e)
	{
		Button button = (Button)sender!;
		button.BorderBrush = Brushes.Black; // can't overwrite hover border :(
		button.Background = SideScrollTheme.ToolbarButtonBackgroundPointerOver;
	}

	private static void Button_PointerExited(object? sender, PointerEventArgs e)
	{
		Button button = (Button)sender!;
		button.Background = SideScrollTheme.TabBackground;
		//button.Background = Brushes.Transparent;
		button.BorderBrush = button.Background;
	}
}
