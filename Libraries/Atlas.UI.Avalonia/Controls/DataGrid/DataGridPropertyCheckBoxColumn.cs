using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Reflection;

namespace Atlas.UI.Avalonia
{
	public class DataGridPropertyCheckBoxColumn : DataGridCheckBoxColumn
	{
		private Binding formattedBinding;
		public PropertyInfo propertyInfo;

		public DataGridPropertyCheckBoxColumn(PropertyInfo propertyInfo, bool isReadOnly)
		{
			this.propertyInfo = propertyInfo;
			IsReadOnly = isReadOnly;
			Binding = GetBinding();
			CanUserSort = true;
		}

		public override string ToString() => propertyInfo.Name;

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			var checkbox = GenerateEditingElementDirect(cell, dataItem);
			if (Binding != null)
				checkbox.Bind(CheckBox.IsCheckedProperty, Binding);

			/*var checkbox = new CheckBox()
			{
				Margin = new Thickness(10, 0, 0, 0), // aligns with header title better than centering
			};
			//unformattedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			if (IsReadOnly)
				checkbox.IsHitTestVisible = false; // disable changing*/
			return checkbox;
		}

		private Binding GetBinding()
		{
			Binding binding = Binding as Binding ?? new Binding(propertyInfo.Name);

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