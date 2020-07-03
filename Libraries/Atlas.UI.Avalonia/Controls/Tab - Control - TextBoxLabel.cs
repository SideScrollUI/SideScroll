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
	public class TabControlTextBoxLabel : TextBox, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(TextBox);

		public TabControlTextBoxLabel(string text)
		{
			if (text != null)
				Text = text;
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			Foreground = Theme.BackgroundText;
			Background = Brushes.Transparent;
			BorderThickness = new Thickness(0);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			IsReadOnly = true;
			FontSize = 16;
			Padding = new Thickness(6, 3);
			//Margin = new Thickness(4);
			//Focusable = true, // already set?
			MinWidth = 50;
			MaxWidth = 1000;
			TextWrapping = TextWrapping.Wrap;

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
