using Avalonia.Controls;
using Avalonia.Data;
using SideScroll.Avalonia.Themes;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Controls;

public class TabTextButton : Button
{
	protected override Type StyleKeyOverride => typeof(Button);

	public TabTextButton(string? label = null, AccentType accentType = default)
	{
		Content = label;
		if (accentType == AccentType.Warning)
		{
			UseWarningAccent();
		}
	}

	public void UseWarningAccent()
	{
		Resources.Add("ButtonBackground", SideScrollTheme.ButtonWarningBackground);
		Resources.Add("ButtonForeground", SideScrollTheme.ButtonWarningForeground);
		Resources.Add("ButtonBorderBrush", SideScrollTheme.ButtonWarningBorder);

		Resources.Add("ButtonBackgroundPointerOver", SideScrollTheme.ButtonWarningBackgroundPointerOver);
		Resources.Add("ButtonBackgroundPressed", SideScrollTheme.ButtonWarningBackgroundPointerOver);

		Resources.Add("ButtonForegroundPointerOver", SideScrollTheme.ButtonWarningForegroundPointerOver);
		Resources.Add("ButtonForegroundPressed", SideScrollTheme.ButtonWarningForegroundPointerOver);

		Resources.Add("ButtonBorderBrushPointerOver", SideScrollTheme.ButtonWarningBorderPointerOver);
		Resources.Add("ButtonBorderBrushPressed", SideScrollTheme.ButtonWarningBorderPointerOver);
	}

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
