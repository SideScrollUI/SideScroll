using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace SideScroll.UI.Avalonia.Controls;

public class TabControlTextButton : Button
{
	protected override Type StyleKeyOverride => typeof(Button);

	public TabControlTextButton(string? label = null)
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
		this.Bind(IsVisibleProperty, binding);
	}
}
