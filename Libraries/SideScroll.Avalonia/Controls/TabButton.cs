using Avalonia.Controls;
using Avalonia.Data;

namespace SideScroll.Avalonia.Controls;

/// <summary>
/// A styled Avalonia button used within SideScroll tab controls.
/// </summary>
public class TabButton : Button
{
	public TabButton(string? label = null)
	{
		Content = label;
	}

	/// <summary>Binds the button's visibility to a property on the current data context.</summary>
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
