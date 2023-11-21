using Atlas.Tabs;
using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;

namespace Atlas.UI.Avalonia.Controls;

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
		Bind(property);
	}

	private void Initialize()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		BorderThickness = new Thickness(1);
		MaxWidth = TabControlParams.ControlMaxWidth;
		//Margin = new Thickness(2, 2);
		//Padding = new Thickness(6, 3);

		//Resources.Add("CheckBoxCheckBackgroundFillUnchecked", Theme.Background);
	}

	private void Bind(ListProperty property)
	{
		var binding = new Binding(property.PropertyInfo.Name)
		{
			Mode = BindingMode.TwoWay,
			Source = property.Object,
		};
		this.Bind(IsCheckedProperty, binding);
	}
}
