using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using SideScroll.Tabs.Lists;
using SideScroll.UI.Avalonia.Themes;
using SideScroll.UI.Avalonia.Utilities;
using System.ComponentModel;

namespace SideScroll.UI.Avalonia.Controls;

public class TabControlColorPicker : ColorPicker
{
	protected override Type StyleKeyOverride => typeof(ColorPicker);

	public ListProperty? Property;

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
	}

	private void TabControlColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
	{
		ToolTip.SetTip(this, e.NewColor.ToString());
	}

	private void Bind(ListProperty property)
	{
		var binding = new Binding(property.PropertyInfo.Name)
		{
			Mode = BindingMode.TwoWay,
			Source = property.Object,
		};
		this.Bind(ColorProperty, binding);

		if (property.Object is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
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
