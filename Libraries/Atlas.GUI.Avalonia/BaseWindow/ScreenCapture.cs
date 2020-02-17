using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;

namespace Atlas.GUI.Avalonia
{
	public class ScreenCapture : Grid
	{
		private RenderTargetBitmap correctedBitmap;
		private RenderTargetBitmap selectionBitmap;
		private Image selectionImage;

		public ScreenCapture(IVisual visual)
		{
			InitializeComponent(visual);
		}

		private void InitializeComponent(IVisual visual)
		{
			Background = Brushes.Black;
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;

			// visual is offset by the Toolbar height
			var bounds = visual.Bounds;
			var sourceBitmap = new RenderTargetBitmap(new PixelSize((int)bounds.Right, (int)bounds.Bottom), new Vector(96, 96));
			sourceBitmap.Render(visual);

			correctedBitmap = new RenderTargetBitmap(new PixelSize((int)bounds.Width, (int)bounds.Height), new Vector(96, 96));

			using (var ctx = correctedBitmap.CreateDrawingContext(null))
			{
				var destRect = new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
				ctx.DrawImage(sourceBitmap.PlatformImpl, 0.85, bounds, destRect);
			}

			var image = new Image()
			{
				Source = correctedBitmap,
				Cursor = new Cursor(StandardCursorType.Cross),
				[Grid.RowProperty] = 1,
			};
			Children.Add(image);

			selectionImage = new Image()
			{
				[Grid.RowProperty] = 1,
			};

			Border selectionBorder = new Border()
			{
				BorderThickness = new Thickness(1),
				BorderBrush = Brushes.Red,
				Child = selectionImage,
			};

			Children.Add(selectionBorder);

			//rtb.Save(Path.Combine(OutputPath, testName + ".out.png"));

			PointerPressed += ScreenCapture_PointerPressed;
		}

		private Point? startPoint;
		private Rect selectionRect;

		private void ScreenCapture_PointerPressed(object sender, PointerPressedEventArgs e)
		{
			if (startPoint == null)
			{
				startPoint = e.GetPosition(this);
				PointerMoved += ScreenCapture_PointerMoved;
			}
			else
			{
				PointerMoved -= ScreenCapture_PointerMoved;
				var bitmap = GetSelectedBitmap();
				//((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(bitmap);
			}
		}

		private RenderTargetBitmap GetSelectedBitmap()
		{
			var destRect = new Rect(0, 0, selectionRect.Width, selectionRect.Height);

			var bitmap = new RenderTargetBitmap(new PixelSize((int)destRect.Width, (int)destRect.Height), new Vector(96, 96));

			using (var ctx = bitmap.CreateDrawingContext(null))
			{
				ctx.DrawImage(correctedBitmap.PlatformImpl, 1, selectionRect, destRect);
			};
			return bitmap;
		}

		private void ScreenCapture_PointerMoved(object sender, PointerEventArgs e)
		{
			var mousePosition = e.GetPosition(this);

			Point topLeft = new Point(Math.Min(startPoint.Value.X, mousePosition.X), Math.Min(startPoint.Value.Y, mousePosition.Y));
			Point bottomRight = new Point(Math.Max(startPoint.Value.X, mousePosition.X), Math.Max(startPoint.Value.Y, mousePosition.Y));

			selectionRect = new Rect(topLeft, bottomRight);
			//var destRect = new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);

			Size sourceSize = correctedBitmap.Size;

			selectionBitmap = new RenderTargetBitmap(new PixelSize((int)sourceSize.Width, (int)sourceSize.Height), new Vector(96, 96));

			using (var ctx = selectionBitmap.CreateDrawingContext(null))
			{
				ctx.DrawImage(correctedBitmap.PlatformImpl, 1, selectionRect, selectionRect);
			}
			selectionImage.Source = selectionBitmap;
		}
	}
}
