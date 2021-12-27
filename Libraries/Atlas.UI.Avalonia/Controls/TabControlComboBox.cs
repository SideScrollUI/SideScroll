using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using System;
using System.Collections;
using System.Reflection;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlComboBox : ComboBox, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(ComboBox);

		public ListProperty Property;

		public TabControlComboBox()
		{
			InitializeComponent();
		}

		public TabControlComboBox(ListProperty property, BindListAttribute propertyListAttribute)
		{
			Property = property;

			InitializeComponent();

			IsEnabled = property.Editable;
			MaxWidth = TabControlParams.ControlMaxWidth;

			Type type = property.UnderlyingType;

			if (propertyListAttribute != null)
			{
				PropertyInfo propertyInfo = property.Object.GetType().GetProperty(propertyListAttribute.Name);
				Items = propertyInfo.GetValue(property.Object) as IEnumerable;
			}
			else
			{
				var values = type.GetEnumValues();
				Items = values;
			}
			Bind(property.Object, property.PropertyInfo.Name);
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

			if ((obj == null || SelectedItem == null) && Items.GetEnumerator().MoveNext())
				SelectedIndex = 0;
		}

		private void InitializeComponent()
		{
			//MaxWidth = TabControlParams.ControlMaxWidth;

			HorizontalAlignment = HorizontalAlignment.Stretch;
		}
	}
}
