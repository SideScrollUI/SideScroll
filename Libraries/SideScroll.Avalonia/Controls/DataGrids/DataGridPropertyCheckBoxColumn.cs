using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

/// <summary>A data-grid check-box column bound to a reflected <see cref="PropertyInfo"/>, with optional cell styling for horizontal grid-line emulation.</summary>
public class DataGridPropertyCheckBoxColumn : DataGridCheckBoxColumn
{
	/// <summary>Gets the reflected property this column is bound to.</summary>
	public PropertyInfo PropertyInfo { get; }

	/// <summary>Gets or sets whether any column in the grid uses cell styling, which requires this column to manually draw horizontal lines.</summary>
	public bool StyleCells { get; set; }

	private Binding _formattedBinding;

	/// <summary>Returns the reflected property name.</summary>
	public override string ToString() => PropertyInfo.Name;

	public DataGridPropertyCheckBoxColumn(PropertyInfo propertyInfo, bool isReadOnly)
	{
		PropertyInfo = propertyInfo;
		IsReadOnly = isReadOnly;
		Binding = GetBinding();
		CanUserSort = true;
	}

	protected override Control GenerateElement(DataGridCell cell, object dataItem)
	{
		if (StyleCells && DisplayIndex > 0)
		{
			cell.BorderThickness = new Thickness(1, 0, 0, 1); // Left and Bottom
		}
		else
		{
			cell.BorderThickness = new Thickness(0, 0, 1, 1); // Right and Bottom
		}

		var checkBox = (CheckBox)GenerateEditingElementDirect(cell, dataItem);
		if (Binding != null)
		{
			checkBox.Bind(ToggleButton.IsCheckedProperty, Binding);
		}

		checkBox.Margin = new Thickness(10, 0, 4, 0); // Checkbox isn't centered (due to optional text control?)
		checkBox.IsEnabled = !IsReadOnly;
		checkBox.HorizontalAlignment = HorizontalAlignment.Center;
		checkBox.Resources.Add("CheckBoxCheckBackgroundFillUnchecked", Brushes.Transparent);
		/*var checkbox = new CheckBox()
		{
			Margin = new Thickness(10, 0, 0, 0), // aligns with header title better than centering
		};
		//unformattedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
		if (IsReadOnly)
			checkbox.IsHitTestVisible = false; // disable changing*/

		return checkBox;
	}

	[MemberNotNull(nameof(_formattedBinding))]
	private Binding GetBinding()
	{
		Binding binding = Binding as Binding ?? new Binding(PropertyInfo.Name);

		_formattedBinding ??= new Binding
		{
			Path = binding.Path,
			Mode = BindingMode.TwoWay, // copying a value to the clipboard triggers an infinite loop without this?
		};

		return _formattedBinding;
	}
}
