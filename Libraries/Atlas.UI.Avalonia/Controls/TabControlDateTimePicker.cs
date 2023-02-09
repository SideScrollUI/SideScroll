using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Atlas.UI.Avalonia.Controls;

public class TabCalendarDatePicker : CalendarDatePicker, IStyleable
{
	Type IStyleable.StyleKey => typeof(CalendarDatePicker);

	// Default behavior increments and decrements Date when scrolling left/right with the mousepad
	// This is probably useful for Mobile devices, but not Desktop
	// Override and do nothing instead
	protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
	{
	}
}

public class TabDateTimePicker : Grid, IStyleable
{
	Type IStyleable.StyleKey => typeof(TabDateTimePicker);

	public readonly ListProperty Property;

	public Binding Binding { get; set; }

	private DateTimeValueConverter _dateTimeConverter;
	private TabCalendarDatePicker _datePicker;
	private TabControlTextBox _timeTextBox;

	public TabDateTimePicker(ListProperty property)
	{
		Property = property;

		InitializeComponent();
	}

	[MemberNotNull(nameof(Binding), nameof(_dateTimeConverter), nameof(_datePicker), nameof(_timeTextBox))]
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

		if (Property.Editable)
		{
			Button buttonImport = AddButton("Import Clipboard", Icons.Streams.Paste);
			buttonImport.Click += ButtonImport_Click;
			Children.Add(buttonImport);
		}
	}

	[MemberNotNull(nameof(_datePicker))]
	private void AddDatePicker()
	{
		_datePicker = new TabCalendarDatePicker()
		{
			Background = Property.Editable ? Theme.Background : Brushes.LightGray,
			BorderBrush = Brushes.Black,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalContentAlignment = VerticalAlignment.Center,
			BorderThickness = new Thickness(1),
			SelectedDateFormat = CalendarDatePickerFormat.Custom,
			CustomDateFormatString = "yyyy/M/d",
			Watermark = "yyyy/M/d",
			MinWidth = 105,
			MaxWidth = 300,
			IsEnabled = Property.Editable,
		};

		if (!Property.Editable)
		{
			_datePicker.Background = Theme.TextBackgroundDisabled;
		}

		_datePicker.Bind(CalendarDatePicker.SelectedDateProperty, Binding);
		Children.Add(_datePicker);
	}

	[MemberNotNull(nameof(_timeTextBox))]
	private void AddTimeTextBox()
	{
		_timeTextBox = new TabControlTextBox()
		{
			IsReadOnly = !Property.Editable,
			Watermark = "15:30:45",
			Margin = new Thickness(10, 0, 0, 0),
			MinWidth = 75,
			MaxWidth = 300,
			Focusable = true, // already set?
			[Grid.ColumnProperty] = 1,
		};

		if (!Property.Editable)
		{
			_timeTextBox.Background = Theme.TextBackgroundDisabled;
			_timeTextBox.Foreground = Theme.ForegroundLight;
		}

		_timeTextBox.Bind(TextBlock.TextProperty, Binding);
		Children.Add(_timeTextBox);
	}

	private void ButtonImport_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		string clipboardText = ClipBoardUtils.GetTextAsync().Result;
		TimeSpan? timeSpan = DateTimeUtils.ConvertTextToTimeSpan(clipboardText);
		if (timeSpan != null)
		{
			DateTime? newDateTime = _dateTimeConverter.Convert(timeSpan, typeof(string), null, CultureInfo.InvariantCulture) as DateTime?;
			Property.PropertyInfo.SetValue(Property.Object, newDateTime);
			_timeTextBox.Text = timeSpan.ToString()!;
			e.Handled = true;
		}
		else
		{
			DateTime? dateTime = DateTimeUtils.ConvertTextToDateTime(clipboardText);
			if (dateTime != null)
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
	private void Button_PointerEnter(object? sender, PointerEventArgs e)
	{
		Button button = (Button)sender!;
		button.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
		button.Background = Theme.ToolbarButtonBackgroundHover;
	}

	private void Button_PointerLeave(object? sender, PointerEventArgs e)
	{
		Button button = (Button)sender!;
		button.Background = Theme.TabBackground;
		//button.Background = Brushes.Transparent;
		button.BorderBrush = button.Background;
	}
}
