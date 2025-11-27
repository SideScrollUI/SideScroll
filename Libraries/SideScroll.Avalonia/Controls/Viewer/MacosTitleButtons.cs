using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace SideScroll.Avalonia.Controls;

public class MacosTitleButtons : TemplatedControl
{
	private Button? CloseButton { get; set; }
	private Button? MinimizeButton { get; set; }
	private Button? ZoomButton { get; set; }

	private Rect? _normalBounds;

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);

		CloseButton = e.NameScope.Find<Button>("CloseButton");
		MinimizeButton = e.NameScope.Find<Button>("MinimizeButton");
		ZoomButton = e.NameScope.Find<Button>("ZoomButton");

		// Attach handlers directly
		if (CloseButton != null)
		{
			CloseButton.Click += CloseWindow;
		}
		if (MinimizeButton != null)
		{
			MinimizeButton.Click += MinimizeWindow;
		}
		if (ZoomButton != null)
		{
			ZoomButton.Click += MaximizeWindow;
		}
	}

	private void CloseWindow(object? sender, RoutedEventArgs e)
	{
		if (VisualRoot is Window window)
		{
			window.Close();
		}
	}

	private void MinimizeWindow(object? sender, RoutedEventArgs e)
	{
		if (VisualRoot is Window window)
		{
			window.WindowState = WindowState.Minimized;
		}
	}

	private void MaximizeWindow(object? sender, RoutedEventArgs e)
	{
		if (VisualRoot is Window window)
		{
			if (window.WindowState == WindowState.Normal)
			{
				_normalBounds = new(
					x: window.Position.X,
					y: window.Position.Y,
					width: window.Width,
					height: window.Height
				);
				window.WindowState = WindowState.Maximized;
			}
			else
			{
				window.WindowState = WindowState.Normal;
				if (_normalBounds is Rect rect)
				{
					window.Width = rect.Width;
					window.Height = rect.Height;
					window.Position = new PixelPoint((int)rect.X, (int)rect.Y);
				}
			}
		}
	}
}
