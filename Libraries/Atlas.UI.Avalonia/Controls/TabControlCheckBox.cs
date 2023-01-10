using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlCheckBox : CheckBox, IStyleable, ILayoutable
{
	Type IStyleable.StyleKey => typeof(CheckBox);

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
		Background = new SolidColorBrush(Colors.White);
		BorderBrush = new SolidColorBrush(Colors.Black);
		HorizontalAlignment = HorizontalAlignment.Stretch;
		BorderThickness = new Thickness(1);
		MaxWidth = TabControlParams.ControlMaxWidth;
		Margin = new Thickness(2, 2);
		//Padding = new Thickness(6, 3);
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
