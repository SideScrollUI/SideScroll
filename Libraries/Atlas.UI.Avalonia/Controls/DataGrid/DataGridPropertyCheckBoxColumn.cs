using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Reflection;

namespace Atlas.UI.Avalonia
{
	public class DataGridPropertyCheckBoxColumn : DataGridCheckBoxColumn
	{
		public PropertyInfo PropertyInfo;

		private Binding formattedBinding;

		public DataGridPropertyCheckBoxColumn(PropertyInfo propertyInfo, bool isReadOnly)
		{
			PropertyInfo = propertyInfo;
			IsReadOnly = isReadOnly;
			Binding = GetBinding();
			CanUserSort = true;
		}

		public override string ToString() => PropertyInfo.Name;

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			var checkbox = (CheckBox)GenerateEditingElementDirect(cell, dataItem);
			if (Binding != null)
				checkbox.Bind(CheckBox.IsCheckedProperty, Binding);

			checkbox.Margin = new Thickness(10, 4);
			checkbox.IsEnabled = !IsReadOnly;
			/*var checkbox = new CheckBox()
			{
				Margin = new Thickness(10, 0, 0, 0), // aligns with header title better than centering
			};
			//unformattedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			if (IsReadOnly)
				checkbox.IsHitTestVisible = false; // disable changing*/

			return new Border()
			{
				BorderThickness = new Thickness(0, 0, 0, 1), // Bottom only
				BorderBrush = Theme.ThemeBorderHighBrush,
				Child = checkbox,
			};
		}

		private Binding GetBinding()
		{
			Binding binding = Binding as Binding ?? new Binding(PropertyInfo.Name);

			if (formattedBinding == null)
			{
				formattedBinding = new Binding
				{
					Path = binding.Path,
					Mode = BindingMode.TwoWay, // copying a value to the clipboard triggers an infinite loop without this?
				};
			}

			return formattedBinding;
		}
	}
}