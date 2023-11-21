using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Atlas.UI.Avalonia;

public class DataGridPropertyCheckBoxColumn : DataGridCheckBoxColumn
{
	public PropertyInfo PropertyInfo;

	public bool StyleCells { get; set; } // True if any column has a Style applied, so we can manually draw the horizontal lines

	private Binding _formattedBinding;

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
		cell.BorderBrush = AtlasTheme.GridBorder;
		cell.BorderThickness = new Thickness(0, 0, 1, 1); // Right and Bottom

		var checkBox = (CheckBox)GenerateEditingElementDirect(cell, dataItem);
		if (Binding != null)
			checkBox.Bind(CheckBox.IsCheckedProperty, Binding);

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

		if (_formattedBinding == null)
		{
			_formattedBinding = new Binding
			{
				Path = binding.Path,
				Mode = BindingMode.TwoWay, // copying a value to the clipboard triggers an infinite loop without this?
			};
		}

		return _formattedBinding;
	}
}
