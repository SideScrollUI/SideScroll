﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlButton : Button, IStyleable
	{
		Type IStyleable.StyleKey => typeof(Button);

		public Brush BackgroundBrush = Theme.Brushes.ButtonBackground;
		public Brush ForegroundBrush = Theme.Brushes.ButtonForeground;
		public Brush HoverBrush = Theme.Brushes.ButtonBackgroundHover;

		public TabControlButton(string label = null)
		{
			Content = label;

			InitializeControl();
		}

		public void InitializeControl()
		{
			Background = BackgroundBrush;
			Foreground = ForegroundBrush;
			BorderBrush = new SolidColorBrush(Colors.Black);
			BorderThickness = new Thickness(1);

			PointerEnter += Button_PointerEnter;
			PointerLeave += Button_PointerLeave;
		}

		private void Button_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			//BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			Background = HoverBrush;
		}

		private void Button_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Background = BackgroundBrush;
			//BorderBrush = button.Background;
		}
	}
}
