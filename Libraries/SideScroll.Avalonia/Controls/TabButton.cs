using Avalonia.Controls;
using Avalonia.Data;

namespace SideScroll.Avalonia.Controls;

public class TabButton : Button
{
	public TabButton(string? label = null)
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
