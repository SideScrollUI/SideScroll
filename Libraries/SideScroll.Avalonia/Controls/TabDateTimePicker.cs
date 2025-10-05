using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using SideScroll.Avalonia.Controls.Converters;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tasks;
using SideScroll.Utilities;
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
	public ListProperty Property { get; }

	public Binding Binding { get; set; }

	private readonly DateTimeValueConverter _dateTimeConverter;
	private TabCalendarDatePicker _datePicker;
	private TabTextBox _timeTextBox;

	public TabDateTimePicker(ListProperty property)
	{
		Property = property;

		ColumnDefinitions = new ColumnDefinitions("*,*,Auto,Auto");
		RowDefinitions = new RowDefinitions("Auto");

		_dateTimeConverter = new DateTimeValueConverter();

		Binding = new Binding(Property.PropertyInfo.Name)
		{
			Converter = _dateTimeConverter,
			Mode = property.IsEditable ? BindingMode.TwoWay : BindingMode.OneWay,
			Source = Property.Object,
		};

		AddDatePicker();
		AddTimeTextBox();

		AddButton("Copy to Clipboard", Icons.Svg.Copy, 2, CopyToClipboard);

		if (Property.IsEditable)
		{
			AddButton("Import from Clipboard", Icons.Svg.Import, 3, ImportFromClipboard);
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
			MaxWidth = TabForm.ControlMaxWidth,
			IsEnabled = Property.IsEditable,
		};

		if (!Property.IsEditable)
		{
			_datePicker.Background = SideScrollTheme.TextReadOnlyBackground;
		}

		_datePicker.Bind(CalendarDatePicker.SelectedDateProperty, Binding);
		Children.Add(_datePicker);
	}

	[MemberNotNull(nameof(_timeTextBox))]
	private void AddTimeTextBox()
	{
		_timeTextBox = new TabTextBox
		{
			IsReadOnly = !Property.IsEditable,
			Watermark = "15:30:45",
			Margin = new Thickness(10, 0, 0, 0),
			MinWidth = 75,
			MaxWidth = TabForm.ControlMaxWidth,
			Focusable = true, // already set?
			[Grid.ColumnProperty] = 1,
		};

		if (!Property.IsEditable)
		{
			_timeTextBox.Background = SideScrollTheme.TextReadOnlyBackground;
			_timeTextBox.Foreground = SideScrollTheme.TextReadOnlyForeground;
		}

		_timeTextBox.Bind(TextBlock.TextProperty, Binding);
		Children.Add(_timeTextBox);
	}

	protected TabImageButton AddButton(string tooltip, IResourceView resourcView, int column, CallAction callAction)
	{
		var button = new TabImageButton(tooltip, resourcView, null, 20)
		{
			Padding = new(5),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
			CallAction = callAction,

			[ToolTip.TipProperty] = tooltip,
			[Grid.ColumnProperty] = column,
		};
		Children.Add(button);
		return button;
	}

	private void ImportFromClipboard(Call call)
	{
		string? clipboardText = ClipboardUtils.TryGetText(this);
		if (clipboardText == null) return;

		if (DateTimeUtils.TryParseTimeSpan(clipboardText, out TimeSpan timeSpan))
		{
			DateTime? newDateTime = _dateTimeConverter.Convert(timeSpan, typeof(string), null, CultureInfo.InvariantCulture) as DateTime?;
			Property.PropertyInfo.SetValue(Property.Object, newDateTime);
			_timeTextBox.Text = timeSpan.ToString();
		}
		else
		{
			if (DateTimeUtils.TryParseDateTime(clipboardText, out DateTime dateTime))
			{
				Property.PropertyInfo.SetValue(Property.Object, dateTime);
				_datePicker.SelectedDate = dateTime;
				_timeTextBox.Text = (string)_dateTimeConverter.Convert(dateTime, typeof(string), null, CultureInfo.InvariantCulture)!;
			}
		}
	}

	private void CopyToClipboard(Call call)
	{
		if (Property.Value is DateTime dateTime)
		{
			ClipboardUtils.SetText(this, dateTime.Format(TimeFormatType.Second)!);
		}
	}
}
