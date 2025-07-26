using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using SideScroll.Avalonia.Utilities;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.Reflection;

namespace SideScroll.Avalonia.Controls;

public class TabComboBox : ComboBox
{
	protected override Type StyleKeyOverride => typeof(ComboBox);

	public ListProperty? Property { get; set; }

	public override string? ToString() => SelectedItem?.ToString();

	public TabComboBox()
	{
		InitializeComponent();
	}

	public TabComboBox(IEnumerable items, object? selectedItem = null)
	{
		InitializeComponent();

		ItemsSource = items;
		SelectedItem = selectedItem;
	}

	public TabComboBox(ListProperty property, string? listPropertyName)
	{
		Property = property;
		IsEnabled = property.Editable;

		InitializeComponent();

		if (listPropertyName != null)
		{
			PropertyInfo? propertyInfo = property.Object.GetType().GetProperty(listPropertyName,
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.Static |
				BindingFlags.FlattenHierarchy);

			ArgumentNullException.ThrowIfNull(propertyInfo);

			ItemsSource = propertyInfo.GetValue(property.Object) as IEnumerable;
		}
		else
		{
			ItemsSource = property.UnderlyingType.GetEnumValues();
		}
		Bind(property.Object, property.PropertyInfo.Name);
	}

	private void InitializeComponent()
	{
		MaxWidth = TabForm.ControlMaxWidth;

		HorizontalAlignment = HorizontalAlignment.Stretch;

		AvaloniaUtils.AddContextMenu(this);
	}

	public void Bind(object obj, string path)
	{
		var binding = new Binding(path)
		{
			//Converter = new FormatValueConverter(),
			Mode = BindingMode.TwoWay,
			Source = obj,
		};
		Bind(SelectedItemProperty, binding);

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
