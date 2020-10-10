using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
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

			var binding = new Binding(property.PropertyInfo.Name)
			{
				//Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = property.Object,
			};
			this.Bind(SelectedItemProperty, binding);

			if ((property.Object == null || SelectedItem == null) && Items.GetEnumerator().MoveNext())
				SelectedIndex = 0;
		}

		private void InitializeComponent()
		{
			//MaxWidth = TabControlParams.ControlMaxWidth;

			BorderBrush = new SolidColorBrush(Colors.Black);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			BorderThickness = new Thickness(1);
		}
	}
}
