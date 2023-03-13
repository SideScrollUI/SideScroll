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

		if (listPropertyName != null)
		{
			PropertyInfo propertyInfo = property.Object.GetType().GetProperty(listPropertyName, 
				BindingFlags.Public | BindingFlags.NonPublic | 
				BindingFlags.Instance | BindingFlags.Static)!;
			Items = propertyInfo.GetValue(property.Object) as IEnumerable;
		}
		else
		{
			Items = property.UnderlyingType.GetEnumValues();
		}
		Bind(property.Object, property.PropertyInfo.Name);
	}

	private void InitializeComponent()
	{
		MaxWidth = TabControlParams.ControlMaxWidth;

		HorizontalAlignment = HorizontalAlignment.Stretch;
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

		SelectDefaultValue();
	}

	private void SelectDefaultValue()
	{
		if ((Property?.Object != null && SelectedItem != null) || Items.GetEnumerator().MoveNext() == false) return;

		// Check for null value match
		object? value = Property!.Value;
		foreach (var item in Items)
		{
			if (item == value)
			{
				SelectedItem = item;
				return;
			}
		}

		var enumerator = Items.GetEnumerator();
		if (enumerator.MoveNext())
		{
			SelectedItem = enumerator.Current;
		}
	}
}
