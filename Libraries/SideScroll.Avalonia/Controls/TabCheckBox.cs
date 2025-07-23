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
		Initialize();
	}

	public TabCheckBox(ListProperty property)
	{
		Initialize();
		IsEnabled = property.Editable;
		Bind(property);
	}

	private void Initialize()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		MaxWidth = TabObjectEditor.ControlMaxWidth;
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
