using Avalonia.Controls;
using Avalonia.Data;

namespace SideScroll.UI.Avalonia.Controls;

public class TabControlButton : Button
{
	public TabControlButton(string? label = null)
	{
		Content = label;
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
