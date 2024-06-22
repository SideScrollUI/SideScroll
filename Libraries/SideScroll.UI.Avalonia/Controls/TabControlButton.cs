using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace SideScroll.UI.Avalonia.Controls;

public class TabControlButton : Button
{
	protected override Type StyleKeyOverride => typeof(TabControlButton);

	public TabControlButton(string? label = null)
	{
		Content = label;

		BorderBrush = Brushes.Black;
	}

	public void BindVisible(string propertyName)
	{
		var binding = new Binding(propertyName)
		{
			Path = propertyName,
			Mode = BindingMode.OneWay,
		};
		this.Bind(IsVisibleProperty, binding);
	}
}
