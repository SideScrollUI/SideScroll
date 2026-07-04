using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using SideScroll.Attributes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.Reflection;

namespace SideScroll.Avalonia.Controls;

/// <summary>A combo box bound to a <see cref="ListProperty"/>, with support for attribute-based enabled items and auto-select of a default value.</summary>
public class TabComboBox : ComboBox
{
	/// <inheritdoc/>
	protected override Type StyleKeyOverride => typeof(ComboBox);

	/// <summary>Gets the list property this combo box is bound to, or <c>null</c> if unbound.</summary>
	public ListProperty? Property { get; }

	/// <summary>Returns the string representation of the currently selected item.</summary>
	public override string? ToString() => SelectedItem?.ToString();

	/// <summary>Creates an empty, unbound combo box.</summary>
	public TabComboBox()
	{
		InitializeComponent();
	}

	/// <summary>Creates a combo box populated with the given items and an optional initial selection.</summary>
	/// <param name="items">The items to display.</param>
	/// <param name="selectedItem">The item to select initially, or <c>null</c> for no selection.</param>
	public TabComboBox(IEnumerable items, object? selectedItem = null)
	{
		InitializeComponent();

		ItemsSource = items;
		SelectedItem = selectedItem;
	}

	/// <summary>Creates a combo box bound to <paramref name="property"/>, optionally sourcing its items from another list property on the same object.</summary>
	/// <param name="property">The list property to bind the selected value to.</param>
	/// <param name="listPropertyName">The name of a property providing the selectable items, or <c>null</c> to use the property's own item source.</param>
	public TabComboBox(ListProperty property, string? listPropertyName)
	{
		Property = property;
		IsEnabled = property.IsEditable;

		InitializeComponent();

		if (property.GetCustomAttribute<ToolTipAttribute>() is { } toolTipAttribute)
		{
			ToolTip.SetTip(this, toolTipAttribute.Text);
		}

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

	/// <summary>Binds the combo box's selected item two-way to the specified property path on <paramref name="obj"/>.</summary>
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
		using var enumerator = Items.GetEnumerator();
		if ((Property?.Object != null && SelectedItem != null) || !enumerator.MoveNext()) return;

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

		SelectedItem = enumerator.Current;
	}
}
