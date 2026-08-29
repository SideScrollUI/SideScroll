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

/// <summary>
/// A color picker that binds to a <see cref="ListProperty"/>, persisting the last-used color model and tab selection across instances.
/// </summary>
public class TabColorPicker : ColorPicker, IDisposable
{
	/// <inheritdoc/>
	protected override Type StyleKeyOverride => typeof(ColorPicker);

	/// <summary>Gets the list property this color picker is bound to, or <c>null</c> if unbound.</summary>
	public ListProperty? Property { get; }

	private INotifyPropertyChanged? _boundNotifier;

	private static int? _prevSelectedIndex = 2;
	private static ColorModel? _prevColorModel = ColorModel.Hsva;

	/// <summary>Creates an unbound color picker with the default styling and restored color model and tab selection.</summary>
	public TabColorPicker()
	{
		HexInputAlphaPosition = AlphaComponentPosition.Leading;
		HorizontalAlignment = HorizontalAlignment.Stretch;
		BorderThickness = new Thickness(1);
		MaxWidth = TabForm.ControlMaxWidth;

		LoadTheme();

		ActualThemeVariantChanged += (_, _) => LoadTheme();

		AvaloniaUtils.AddContextMenu(this);

		ColorChanged += TabColorPicker_ColorChanged;
		PropertyChanged += TabColorPicker_PropertyChanged;
	}

	/// <summary>
	/// Re-resolves the theme overrides. A resource dictionary entry holds the brush it was given
	/// rather than re-resolving it, so these are replaced whenever the theme changes
	/// </summary>
	private void LoadTheme()
	{
		Resources["ComboBoxDropDownGlyphForeground"] = SideScrollTheme.ButtonForeground;
		Resources["TextControlForegroundDisabled"] = SideScrollTheme.ButtonForeground;
	}

	/// <summary>Creates a color picker bound to <paramref name="property"/>.</summary>
	/// <param name="property">The list property to bind to.</param>
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
			_boundNotifier = notifyPropertyChanged;
			notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
		}
	}

	/// <summary>
	/// Releases the subscription to the bound object's property changes
	/// </summary>
	/// <remarks>
	/// The bound object outlives this control, so the event would otherwise keep the picker and
	/// everything it references alive. TabSplitGrid disposes controls when clearing them
	/// </remarks>
	public void Dispose()
	{
		if (_boundNotifier != null)
		{
			_boundNotifier.PropertyChanged -= OnPropertyChanged;
			_boundNotifier = null;
		}

		ColorChanged -= TabColorPicker_ColorChanged;
		PropertyChanged -= TabColorPicker_PropertyChanged;

		GC.SuppressFinalize(this);
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
				_prevSelectedIndex is { } selectedIndex &&
				_prevColorModel is { } colorModel)
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
		if (e.PropertyName == Property?.Name && Property?.Value is { } obj)
		{
			SetCurrentValue(ColorProperty, obj);
		}
	}
}
