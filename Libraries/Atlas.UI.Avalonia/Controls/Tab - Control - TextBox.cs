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
using System.Reflection;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlTextBox : TextBox, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(TextBox);

		public TabControlTextBox()
		{
			InitializeComponent();
		}

		public TabControlTextBox(ListProperty property)
		{
			InitializeComponent();

			IsReadOnly = !property.Editable;
			if (IsReadOnly)
				Background = Theme.TextBackgroundDisabled;

			PasswordCharAttribute passwordCharAttribute = property.propertyInfo.GetCustomAttribute<PasswordCharAttribute>();
			if (passwordCharAttribute != null)
				PasswordChar = passwordCharAttribute.Character;

			ExampleAttribute attribute = property.propertyInfo.GetCustomAttribute<ExampleAttribute>();
			if (attribute != null)
				Watermark = attribute.Text;

			var binding = new Binding(property.propertyInfo.Name)
			{
				Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Source = property.obj,
			};
			Type type = property.UnderlyingType;
			if (type == typeof(string) || type.IsPrimitive)
				binding.Mode = BindingMode.TwoWay;
			else
				binding.Mode = BindingMode.OneWay;
			this.Bind(TextBlock.TextProperty, binding);
			AvaloniaUtils.AddTextBoxContextMenu(this);
		}

		private void InitializeComponent()
		{
			Background = Theme.Background;
			BorderBrush = new SolidColorBrush(Colors.Black);
			BorderThickness = new Thickness(1);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			MinWidth = 50;
			Padding = new Thickness(6, 3);
			Focusable = true; // already set?
			MaxWidth = TabControlParams.ControlMaxWidth;
			//TextWrapping = TextWrapping.Wrap, // would be a useful feature if it worked
		}
	}
}
