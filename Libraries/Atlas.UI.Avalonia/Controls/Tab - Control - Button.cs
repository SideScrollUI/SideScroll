using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlButton : Button, IStyleable
	{
		Type IStyleable.StyleKey => typeof(TabControlButton);

		public Brush BackgroundBrush = Theme.ButtonBackground;
		public Brush ForegroundBrush = Theme.ButtonForeground;
		public Brush HoverBrush = Theme.ButtonBackgroundHover;

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

		public void BindVisible(string propertyName)
		{
			var binding = new Binding(propertyName)
			{
				//Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Path = propertyName,
				Mode = BindingMode.TwoWay,
				//Source = DataContext,
			};
			((Button)this).Bind(IsVisibleProperty, binding);
		}
	}
}
