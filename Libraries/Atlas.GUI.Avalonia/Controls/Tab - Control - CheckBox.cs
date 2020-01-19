using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlCheckBox : CheckBox, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(CheckBox);

		public TabControlCheckBox()
		{
			Initialize();
		}

		public TabControlCheckBox(ListProperty property)
		{
			Initialize();
			Bind(property);
		}

		private void Bind(ListProperty property)
		{
			var binding = new Binding(property.propertyInfo.Name)
			{
				//Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Mode = BindingMode.TwoWay,
				Source = property.obj,
			};
			((CheckBox)this).Bind(IsCheckedProperty, binding);
		}

		private void Initialize()
		{
			Background = new SolidColorBrush(Colors.White);
			BorderBrush = new SolidColorBrush(Colors.Black);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			BorderThickness = new Thickness(1);
			//MinWidth = 50;
			MaxWidth = TabControlParams.ControlMaxWidth;
			Margin = new Thickness(2, 2);
			//Focusable = true; // already set?
			//Padding = new Thickness(6, 3);
		}

		/*private IBrush OriginalColor;

		// DefaultTheme.xaml is setting this for templates
		private void TextBox_PointerEnter(object sender, PointerEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			//textBox.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			if (textBox.IsEnabled && !textBox.IsReadOnly)
			{
				OriginalColor = textBox.Background;
				textBox.Background = new SolidColorBrush(Theme.ControlBackgroundHover);
			}
		}

		private void TextBox_PointerLeave(object sender, PointerEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (textBox.IsEnabled && !textBox.IsReadOnly)
				textBox.Background = OriginalColor ?? textBox.Background;
			//textBox.BorderBrush = textBox.Background;
		}*/
	}
}
