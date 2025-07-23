using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using SideScroll.Tabs.Lists;

namespace SideScroll.Avalonia.Controls;

public class TabControlCheckBox : CheckBox
{
	protected override Type StyleKeyOverride => typeof(CheckBox);

	public TabControlCheckBox()
	{
		Initialize();
	}

	public TabControlCheckBox(ListProperty property)
	{
		Initialize();
		IsEnabled = property.Editable;
		Bind(property);
	}

	private void Initialize()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		MaxWidth = TabControlParams.ControlMaxWidth;
		//Margin = new Thickness(2, 2);
		//Padding = new Thickness(6, 3);
	}

	private void Bind(ListProperty property)
	{
		var binding = new Binding(property.PropertyInfo.Name)
		{
			Mode = property.Editable ? BindingMode.TwoWay : BindingMode.OneWay,
			Source = property.Object,
		};
		Bind(IsCheckedProperty, binding);
	}
}
