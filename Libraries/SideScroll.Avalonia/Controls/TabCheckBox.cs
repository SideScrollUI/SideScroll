using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using SideScroll.Tabs.Lists;

namespace SideScroll.Avalonia.Controls;

public class TabCheckBox : CheckBox
{
	protected override Type StyleKeyOverride => typeof(CheckBox);

	public TabCheckBox()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		MaxWidth = TabForm.ControlMaxWidth;
		//Margin = new Thickness(2, 2);
		//Padding = new Thickness(6, 3);
	}

	public TabCheckBox(ListProperty property) : this()
	{
		IsEnabled = property.Editable;
		Bind(property);
	}

	public void Bind(ListProperty property)
	{
		var binding = new Binding(property.PropertyInfo.Name)
		{
			Mode = property.Editable ? BindingMode.TwoWay : BindingMode.OneWay,
			Source = property.Object,
		};
		Bind(IsCheckedProperty, binding);
	}
}
