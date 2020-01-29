using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlTextBox : TextBox, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(TextBox);

		public TabControlTextBox()
		{
			Background = new SolidColorBrush(Colors.White);
			BorderBrush = new SolidColorBrush(Colors.Black);
			BorderThickness = new Thickness(1);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			MinWidth = 50;
			Padding = new Thickness(6, 3);
			Focusable = true; // already set?
			MaxWidth = TabControlParams.ControlMaxWidth;
			//TextWrapping = TextWrapping.Wrap, // would be a useful feature if it worked
			//IsReadOnly = !property.Editable;

			PointerEnter += TextBox_PointerEnter;
			PointerLeave += TextBox_PointerLeave;
		}

		private IBrush OriginalColor;

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
		}
	}
}
/*
Todo: replace with Tab Avalonia Edit
*/
