using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlComboBox : ComboBox, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(ComboBox);

		private IBrush OriginalColor;

		public TabControlComboBox()
		{
			Background = new SolidColorBrush(Colors.White);
			BorderBrush = new SolidColorBrush(Colors.Black);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			BorderThickness = new Thickness(1);

			PointerEnter += ComboBox_PointerEnter;
			PointerLeave += ComboBox_PointerLeave;
		}

		// DefaultTheme.xaml is setting this for templates
		private void ComboBox_PointerEnter(object sender, PointerEventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			//textBox.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			if (comboBox.IsEnabled)
			{
				OriginalColor = comboBox.Background;
				comboBox.Background = new SolidColorBrush(Theme.ControlBackgroundHover);
			}
		}

		private void ComboBox_PointerLeave(object sender, PointerEventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			if (comboBox.IsEnabled)
				comboBox.Background = OriginalColor ?? new SolidColorBrush(Colors.White);
			//textBox.BorderBrush = textBox.Background;
		}
	}
}
