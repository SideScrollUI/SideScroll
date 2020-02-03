using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlButton : Button, IStyleable
	{
		Type IStyleable.StyleKey => typeof(Button);

		public TabControlButton(string label)
		{
			Content = label;

			InitializeControl();
		}

		public void InitializeControl()
		{
			Background = new SolidColorBrush(Theme.ButtonBackgroundColor);
			Foreground = new SolidColorBrush(Theme.ButtonForegroundColor);
			BorderBrush = new SolidColorBrush(Colors.Black);
			BorderThickness = new Thickness(1);

			PointerEnter += Button_PointerEnter;
			PointerLeave += Button_PointerLeave;
		}

		private void Button_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			//button.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			button.Background = new SolidColorBrush(Theme.ButtonBackgroundHoverColor);
		}

		private void Button_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = new SolidColorBrush(Theme.ButtonBackgroundColor);
			//button.BorderBrush = button.Background;
		}
	}
}
