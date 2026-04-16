using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using SideScroll.Attributes;
using SideScroll.Tabs.Lists;

namespace SideScroll.Avalonia.Controls;

/// <summary>
/// A styled check box that binds to a <see cref="ListProperty"/>, respecting editable and tooltip attributes.
/// </summary>
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
		IsEnabled = property.IsEditable;

		if (property.GetCustomAttribute<ToolTipAttribute>() is ToolTipAttribute toolTipAttribute)
		{
			ToolTip.SetTip(this, toolTipAttribute.Text);
		}

		Bind(property);
	}

	/// <summary>Binds the check box's checked state to the given list property.</summary>
	public void Bind(ListProperty property)
	{
		var binding = new Binding(property.PropertyInfo.Name)
		{
			Mode = property.IsEditable ? BindingMode.TwoWay : BindingMode.OneWay,
			Source = property.Object,
		};
		Bind(IsCheckedProperty, binding);
	}
}
