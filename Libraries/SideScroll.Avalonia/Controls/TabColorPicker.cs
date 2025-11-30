using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Tabs.Lists;
using System.ComponentModel;

namespace SideScroll.Avalonia.Controls;

public class TabColorPicker : ColorPicker
{
	protected override Type StyleKeyOverride => typeof(ColorPicker);

	public ListProperty? Property { get; }

	private static int? _prevSelectedIndex = 2;
	private static ColorModel? _prevColorModel = ColorModel.Hsva;

	public TabColorPicker()
	{
		HexInputAlphaPosition = AlphaComponentPosition.Leading;
		HorizontalAlignment = HorizontalAlignment.Stretch;
		BorderThickness = new Thickness(1);
		MaxWidth = TabForm.ControlMaxWidth;

		Resources.Add("ComboBoxDropDownGlyphForeground", SideScrollTheme.ButtonForeground);
		Resources.Add("TextControlForegroundDisabled", SideScrollTheme.ButtonForeground);

		AvaloniaUtils.AddContextMenu(this);

		ColorChanged += TabColorPicker_ColorChanged;
		PropertyChanged += TabColorPicker_PropertyChanged;
	}

	public TabColorPicker(ListProperty property) : this()
	{
		Property = property;

		ToolTip.SetTip(this, Property.Value?.ToString());

		Bind(property);
	}

	private void Bind(ListProperty property)
	{
		var binding = new Binding(property.PropertyInfo.Name)
		{
			Mode = property.IsEditable ? BindingMode.TwoWay : BindingMode.OneWay,
			Source = property.Object,
		};
		Bind(ColorProperty, binding);

		if (property.Object is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
		}
	}

	private void TabColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
	{
		ToolTip.SetTip(this, e.NewColor.ToString());
	}

	// Use defaults from previous selections whenever opened
	// This causes all ColorPickers to use the same selections instead of individual ones
	private void TabColorPicker_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (e.Property.Name == nameof(IsKeyboardFocusWithin))
		{
			// Update when focus lost
			if (e.NewValue is false &&
				_prevSelectedIndex is int selectedIndex &&
				_prevColorModel is ColorModel colorModel)
			{
				Dispatcher.UIThread.Post(() =>
				{
					SelectedIndex = selectedIndex;
					ColorModel = colorModel;
				}, DispatcherPriority.Background);
			}
		}
		else if (e.Property.Name == nameof(SelectedIndex))
		{
			if (e.NewValue is int selectedIndex)
			{
				_prevSelectedIndex = selectedIndex;
			}
		}
		else if (e.Property.Name == nameof(ColorModel))
		{
			if (e.NewValue is ColorModel colorModel)
			{
				_prevColorModel = colorModel;
			}
		}
	}

	// Force control to update
	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == Property?.Name && Property?.Value is object obj)
		{
			SetCurrentValue(ColorProperty, obj);
		}
	}
}
