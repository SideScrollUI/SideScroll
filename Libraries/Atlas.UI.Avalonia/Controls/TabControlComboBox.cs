using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using System.Collections;
using System.Reflection;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlComboBox : ComboBox, IStyleable, ILayoutable
{
	Type IStyleable.StyleKey => typeof(ComboBox);

	public ListProperty? Property;

	public TabControlComboBox()
	{
		InitializeComponent();
	}

	public TabControlComboBox(IEnumerable items, object? selectedItem = null)
	{
		InitializeComponent();

		Items = items;
		SelectedItem = selectedItem;
	}

	public TabControlComboBox(ListProperty property, string? listPropertyName)
	{
		Property = property;

		InitializeComponent();

		IsEnabled = property.Editable;
		MaxWidth = TabControlParams.ControlMaxWidth;

		Type type = property.UnderlyingType;

		if (listPropertyName != null)
		{
			PropertyInfo propertyInfo = property.Object.GetType().GetProperty(listPropertyName)!;
			Items = propertyInfo.GetValue(property.Object) as IEnumerable;
		}
		else
		{
			Items = type.GetEnumValues();
		}
		Bind(property.Object, property.PropertyInfo.Name);
	}

	public void Bind(object obj, string path)
	{
		var binding = new Binding(path)
		{
			//Converter = new FormatValueConverter(),
			Mode = BindingMode.TwoWay,
			Source = obj,
		};
		this.Bind(SelectedItemProperty, binding);

		if ((obj == null || SelectedItem == null) && Items.GetEnumerator().MoveNext())
			SelectedIndex = 0;
	}

	private void InitializeComponent()
	{
		//MaxWidth = TabControlParams.ControlMaxWidth;

		HorizontalAlignment = HorizontalAlignment.Stretch;
	}
}
