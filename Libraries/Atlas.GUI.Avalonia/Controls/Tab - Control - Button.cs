using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlButton : Button
	{
		public string Label { get; set; }

		// doesn't work
		private TabControlButton()
		{
			this.Label = "test";

			InitializeControl();
		}

		// doesn't work
		public void InitializeControl()
		{
			Content = Label;
			Background = new SolidColorBrush(Theme.ButtonBackgroundColor);
			Foreground = new SolidColorBrush(Theme.ButtonForegroundColor);
			BorderBrush = new SolidColorBrush(Colors.Black);
			BorderThickness = new Thickness(2);
			// todo: set highlight colors

			//TextColor = Theme.ButtonForegroundColor;
			//BackgroundColor = Theme.ButtonBackgroundColor;
		}

		// works
		public static Button Create(string label)
		{
			Button button = new Button()
			{
				Content = label,
				Background = new SolidColorBrush(Theme.ButtonBackgroundColor),
				Foreground = new SolidColorBrush(Theme.ButtonForegroundColor),
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				// todo: set highlight colors

				//TextColor = Theme.ButtonForegroundColor;
				//BackgroundColor = Theme.ButtonBackgroundColor;
			};
			return button;
		}
	}
}
