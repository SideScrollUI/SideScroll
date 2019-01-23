using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Reflection;

namespace Atlas.GUI.Wpf
{
	public class FormattedCheckBoxColumn : DataGridCheckBoxColumn
	{
		private Binding customBinding;
		private PropertyInfo propertyInfo;
		public event EventHandler<EventArgs> OnModified;

		public FormattedCheckBoxColumn(PropertyInfo propertyInfo)
		{
			this.propertyInfo = propertyInfo;
		}

		protected override FrameworkElement	GenerateElement(DataGridCell cell, object dataItem)
		{
			CheckBox checkbox = base.GenerateEditingElement(cell, dataItem) as CheckBox;

			//CheckBox checkbox = new CheckBox();
			checkbox.SetBinding(CheckBox.IsCheckedProperty, GetBinding());
			if (IsReadOnly)
				checkbox.IsHitTestVisible = false; // enable single click
			else
				checkbox.Click += Checkbox_Click;
			return checkbox;
		}

		protected override FrameworkElement	GenerateEditingElement(DataGridCell cell, object dataItem)
		{
			CheckBox checkbox = base.GenerateEditingElement(cell, dataItem) as CheckBox;
			checkbox.SetBinding(CheckBox.IsCheckedProperty, GetBinding());
			return checkbox;
		}

		Binding	GetBinding()
		{
			Binding binding = (Binding)Binding;
			if (binding == null)
				return new Binding();

			//if (customBinding == null)
			{
				customBinding = new Binding
				{
					Path = binding.Path
				};
				if (!IsReadOnly)
				{
					//customBinding.Mode = BindingMode.TwoWay;
					customBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
					//customBinding.BindsDirectlyToSource = true;
				}
				else
				{
					//customBinding.Mode = BindingMode.OneTime;
				}
				customBinding.Mode = binding.Mode;
			}

			return customBinding;
		}

		private void Checkbox_Click(object sender, RoutedEventArgs e)
		{
			OnModified?.Invoke(this, null);
		}
	}
}