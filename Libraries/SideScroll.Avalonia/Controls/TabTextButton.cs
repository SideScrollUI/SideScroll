using Avalonia.Controls;
using Avalonia.Data;
using SideScroll.Avalonia.Themes;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Controls;

/// <summary>A styled text-label button with optional warning or default accent coloring, used for action buttons in tab forms.</summary>
public class TabTextButton : Button
{
	/// <inheritdoc/>
	protected override Type StyleKeyOverride => typeof(Button);

	/// <summary>Initializes the button with an optional label and accent style.</summary>
	public TabTextButton(string? label = null, AccentType accentType = default)
	{
		Content = label;
		if (accentType == AccentType.Warning)
		{
			UseWarningAccent();
		}
	}

	private bool _warningAccentApplied;

	/// <summary>Applies warning-themed colors (background, foreground, border) to the button.</summary>
	public void UseWarningAccent()
	{
		ApplyWarningAccent();

		if (!_warningAccentApplied)
		{
			_warningAccentApplied = true;

			// The overrides below hold brushes resolved from whichever theme was loaded when they were
			// set, and a resource dictionary entry doesn't re-resolve the way a DynamicResource does,
			// so they have to be replaced whenever the theme changes for the accent to stay current
			ActualThemeVariantChanged += (_, _) => ApplyWarningAccent();
		}
	}

	private void ApplyWarningAccent()
	{
		// Assigned rather than added so re-applying replaces the previous theme's brushes
		Resources["ButtonBackground"] = SideScrollTheme.ButtonWarningBackground;
		Resources["ButtonForeground"] = SideScrollTheme.ButtonWarningForeground;
		Resources["ButtonBorderBrush"] = SideScrollTheme.ButtonWarningBorder;

		Resources["ButtonBackgroundPointerOver"] = SideScrollTheme.ButtonWarningBackgroundPointerOver;
		Resources["ButtonBackgroundPressed"] = SideScrollTheme.ButtonWarningBackgroundPointerOver;

		Resources["ButtonForegroundPointerOver"] = SideScrollTheme.ButtonWarningForegroundPointerOver;
		Resources["ButtonForegroundPressed"] = SideScrollTheme.ButtonWarningForegroundPointerOver;

		Resources["ButtonBorderBrushPointerOver"] = SideScrollTheme.ButtonWarningBorderPointerOver;
		Resources["ButtonBorderBrushPressed"] = SideScrollTheme.ButtonWarningBorderPointerOver;

		Resources["ButtonBackgroundDisabled"] = SideScrollTheme.ButtonWarningBackgroundDisabled;
		Resources["ButtonForegroundDisabled"] = SideScrollTheme.ButtonWarningForegroundDisabled;
		Resources["ButtonBorderBrushDisabled"] = SideScrollTheme.ButtonWarningBorderDisabled;
	}

	/// <summary>Binds <c>IsEnabled</c> one-way to the specified property path on <paramref name="source"/>.</summary>
	public void BindIsEnabled(string path, object? source)
	{
		Bind(IsEnabledProperty, new Binding
		{
			Path = path,
			Source = source,
			Mode = BindingMode.OneWay,
		});
	}

	/// <summary>Binds <c>IsVisible</c> one-way to the specified property path on the DataContext.</summary>
	public void BindVisible(string propertyName)
	{
		var binding = new Binding(propertyName)
		{
			Path = propertyName,
			Mode = BindingMode.OneWay,
		};
		Bind(IsVisibleProperty, binding);
	}
}
