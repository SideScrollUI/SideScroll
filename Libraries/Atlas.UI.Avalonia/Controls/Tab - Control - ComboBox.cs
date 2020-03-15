using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace Atlas.UI.Avalonia.Controls
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

		private void ComboBox_PointerEnter(object sender, PointerEventArgs e)
		{
			if (IsEnabled)
			{
				OriginalColor = Background;
				Background = new SolidColorBrush(Theme.ControlBackgroundHover);
			}
		}

		private void ComboBox_PointerLeave(object sender, PointerEventArgs e)
		{
			if (IsEnabled)
				Background = OriginalColor ?? new SolidColorBrush(Colors.White);
		}
	}
}
