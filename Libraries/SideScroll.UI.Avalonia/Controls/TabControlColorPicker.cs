using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using SideScroll.Tabs.Lists;
using SideScroll.UI.Avalonia.Themes;
using SideScroll.UI.Avalonia.Utilities;
using System.ComponentModel;

namespace SideScroll.UI.Avalonia.Controls;

public class TabControlColorPicker : ColorPicker
{
	protected override Type StyleKeyOverride => typeof(ColorPicker);

	public ListProperty? Property;

	private static int? _prevSelectedIndex;
	private static ColorModel? _prevColorModel;

	public TabControlColorPicker()
	{
		Initialize();
	}

	public TabControlColorPicker(ListProperty property)
	{
		Property = property;

		Initialize();
		Bind(property);
	}

	private void Initialize()
	{
		HexInputAlphaPosition = AlphaComponentPosition.Leading;
		HorizontalAlignment = HorizontalAlignment.Stretch;
		BorderThickness = new Thickness(1);
		MaxWidth = TabControlParams.ControlMaxWidth;

		ToolTip.SetTip(this, Property?.Value?.ToString());

		Resources.Add("ComboBoxDropDownGlyphForeground", SideScrollTheme.ButtonForeground);
		Resources.Add("TextControlForegroundDisabled", SideScrollTheme.ButtonForeground);

		AvaloniaUtils.AddContextMenu(this);

		ColorChanged += TabControlColorPicker_ColorChanged;
		PropertyChanged += TabControlColorPicker_PropertyChanged;
	}

	private void Bind(ListProperty property)
	{
		var binding = new Binding(property.PropertyInfo.Name)
		{
			Mode = BindingMode.TwoWay,
			Source = property.Object,
		};
		Bind(ColorProperty, binding);

		if (property.Object is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
		}
	}

	private void TabControlColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
	{
		ToolTip.SetTip(this, e.NewColor.ToString());
	}

	// Use defaults from previous selections whenever opened
	// This causes all ColorPickers to use the same selections instead of individual ones
	private void TabControlColorPicker_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
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
		if (e.PropertyName == Property?.Name)
		{
			SetCurrentValue(ColorProperty, Property!.Value);
		}
	}
}
