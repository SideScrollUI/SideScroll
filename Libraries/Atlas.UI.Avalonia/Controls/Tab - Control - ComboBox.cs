using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
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

		private ListProperty property;

		public TabControlComboBox()
		{
			InitializeComponent();
		}

		public TabControlComboBox(ListProperty property, BindListAttribute propertyListAttribute)
		{
			this.property = property;
			InitializeComponent();

			IsEnabled = property.Editable;
			MaxWidth = TabControlParams.ControlMaxWidth;
			Type type = property.UnderlyingType;

			if (propertyListAttribute != null)
			{
				PropertyInfo propertyInfo = property.obj.GetType().GetProperty(propertyListAttribute.Name);
				Items = propertyInfo.GetValue(property.obj) as IEnumerable;
			}
			else
			{
				var values = type.GetEnumValues();
				Items = values;
			}

			var binding = new Binding(property.propertyInfo.Name)
			{
				//Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = property.obj,
			};
			this.Bind(SelectedItemProperty, binding);

			if (property.obj == null && Items.GetEnumerator().MoveNext())
				SelectedIndex = 0;
		}

		private void InitializeComponent()
		{
			/*
			BorderThickness = new Thickness(1);
			MinWidth = 50;
			Padding = new Thickness(6, 3);
			Focusable = true; // already set?
			MaxWidth = TabControlParams.ControlMaxWidth;*/
			//TextWrapping = TextWrapping.Wrap, // would be a useful feature if it worked

			Background = Theme.Background;
			BorderBrush = new SolidColorBrush(Colors.Black);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			BorderThickness = new Thickness(1);
		}
	}
}
