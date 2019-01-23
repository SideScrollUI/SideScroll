using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Reflection;

namespace Atlas.GUI.Wpf
{
	public class FormattedTextColumn : DataGridTextColumn
	{
		private Binding formattedBinding;
		private Binding unformattedBinding;
		private FieldValueConverter formatConverter = new FieldValueConverter();
		private PropertyInfo propertyInfo;

		public FormattedTextColumn(PropertyInfo propertyInfo)
		{
			this.propertyInfo = propertyInfo;
		}

		protected override FrameworkElement	GenerateElement(DataGridCell cell, object dataItem)
		{
			if (GetBindingType(dataItem) == typeof(bool))
			{
				CheckBox checkbox = new CheckBox()
				{
					Margin = new Thickness(10, 0, 0, 0)
				};
				GetTextBinding();
				unformattedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				checkbox.SetBinding(CheckBox.IsCheckedProperty, unformattedBinding);
				if (IsReadOnly)
					checkbox.IsHitTestVisible = false; // disable changing
				//formattedBinding = unformattedBinding;
				//Binding = unformattedBinding;
				return checkbox;
			}
			else
			{
				TextBlock element = base.GenerateElement(cell, dataItem) as TextBlock;
				element.SetBinding(TextBlock.TextProperty, GetFormattedTextBinding());
				return element;
			}
		}

		protected override FrameworkElement	GenerateEditingElement(DataGridCell cell, object dataItem)
		{
			if (GetBindingType(dataItem) == typeof(bool))
			{
				CheckBox checkbox = new CheckBox()
				{
					HorizontalAlignment = HorizontalAlignment.Center
				};
				checkbox.SetBinding(CheckBox.IsCheckedProperty, GetTextBinding());
				return checkbox;
			}
			else
			{
				TextBox element = base.GenerateEditingElement(cell, dataItem) as TextBox;
				element.SetBinding(TextBox.TextProperty, GetTextBinding());
				return element;
			}
		}

		private Type GetBindingType(object dataItem)
		{
			if (propertyInfo.GetIndexParameters().Length > 0)
				return null;
			
			if (propertyInfo.DeclaringType.IsAssignableFrom(dataItem.GetType()))
			{
				object obj = propertyInfo.GetValue(dataItem);
				if (obj == null)
					return null;
				return obj.GetType();
			}
			return null;
		}

		Binding	GetTextBinding()
		{
			Binding binding = (Binding)Binding;
			if (binding == null)
				return new Binding();

			//if (unformattedBinding == null)
			{
				unformattedBinding = new Binding
				{
					Path = binding.Path,
				};
				//if (!IsReadOnly)
				//	unformattedBinding.Mode = BindingMode.TwoWay;
				//else
					unformattedBinding.Mode = binding.Mode;
				//unformattedBinding.BindsDirectlyToSource = true;
			}

			return unformattedBinding;
		}

		Binding	GetFormattedTextBinding()
		{
			Binding binding = (Binding)Binding;
			if (binding == null)
				return new Binding();

			if (formattedBinding == null)
			{
				formattedBinding = new Binding
				{
					Path = binding.Path,
					Converter = Formatter,
					Mode = binding.Mode
				};
			}

			return formattedBinding;
		}

		public FieldValueConverter Formatter
		{
			get { return formatConverter; }
			set { formatConverter = value; }
		}
	}
}