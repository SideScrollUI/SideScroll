using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia.Controls
{
	// ReadOnly string control with wordwrap, scrolling, and clipboard copy
	public class TabControlTextBlock : Border, IStyleable, ILayoutable
	{
		public string Text { get; set; }

		public TextBlock TextBlock { get; set; }

		Type IStyleable.StyleKey => typeof(TextBlock);

		public TabControlTextBlock(string text)
		{
			Text = text;

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			Background = Theme.TextBackgroundBrush;
			BorderBrush = Brushes.Black;
			BorderThickness = new Thickness(1);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Top;
			MinWidth = 50;
			MaxWidth = TabControlParams.ControlMaxWidth;
			Margin = new Thickness(6);
			Padding = new Thickness(6, 3);

			TextBlock = new TextBlock()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Foreground = Theme.TitleForeground,
				TextWrapping = TextWrapping.Wrap,
				FontSize = 14,
				Text = Text,
			};
			AddContextMenu();

			var scrollViewer = new ScrollViewer()
			{
				Content = TextBlock,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				BorderThickness = new Thickness(2), // doesn't work
				BorderBrush = Brushes.Black,
				Padding = new Thickness(10),
			};

			Child = scrollViewer;
		}

		// TextBlock control doesn't allow selecting text, so add a Copy command to the context menu
		private void AddContextMenu()
		{
			var contextMenu = new ContextMenu();

			var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();

			var list = new AvaloniaList<object>();

			var menuItemCopy = new MenuItem()
			{
				Header = "_Copy",
				Foreground = Brushes.Black,
			};
			menuItemCopy.Click += delegate
			{
				Task.Run(() => ClipBoardUtils.SetTextAsync(TextBlock.Text));
			};
			list.Add(menuItemCopy);

			contextMenu.Items = list;

			TextBlock.ContextMenu = contextMenu;
		}
	}
}
