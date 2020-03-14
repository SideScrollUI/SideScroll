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

			ColumnDefinitions = new ColumnDefinitions("*,Auto,*,Auto,*");
			RowDefinitions = new RowDefinitions("*,Auto,*,Auto,*");

			AddBackgroundImage(visual);

			selectionImage = new Image()
			{
				//[Grid.ColumnProperty] = 2,
				//[Grid.RowProperty] = 2,
				[Grid.ColumnSpanProperty] = 5,
				[Grid.RowSpanProperty] = 5,
			};
			Children.Add(selectionImage);

			//rtb.Save(Path.Combine(OutputPath, testName + ".out.png"));

			PointerPressed += ScreenCapture_PointerPressed;
		}

		private void AddBackgroundImage(IVisual visual)
		{
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
				//[Grid.RowProperty] = 1,
				[Grid.ColumnSpanProperty] = 5,
				[Grid.RowSpanProperty] = 5,
			};
			Children.Add(image);
		}

		private void AddSplitters()
		{
			AddSplitter(1, 0, HorizontalAlignment.Stretch, VerticalAlignment.Top);
			AddSplitter(0, 1, HorizontalAlignment.Left, VerticalAlignment.Stretch);
			AddSplitter(2, 1, HorizontalAlignment.Right, VerticalAlignment.Stretch);
			AddSplitter(1, 2, HorizontalAlignment.Stretch, VerticalAlignment.Bottom);

			/*Panel panel = new Panel()
			{
				Background = Brushes.Blue,
				[Grid.ColumnProperty] = 2,
				[Grid.RowProperty] = 2,
			};
			Children.Add(panel);*/

			/*Border selectionBorder = new Border()
			{
				BorderThickness = new Thickness(1),
				BorderBrush = Brushes.Red,
				Child = selectionImage,
			};

			Children.Add(selectionBorder);*/
		}

		private void AddSplitter(int column, int row, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
		{
			var splitter = new GridSplitter
			{
				Background = Brushes.White,
				VerticalAlignment = verticalAlignment,
				HorizontalAlignment = horizontalAlignment,
				Width = 1,
				Height = 1,
				[Grid.RowProperty] = row + 1,
				[Grid.ColumnProperty] = column + 1,
			};
			Children.Add(splitter);
			//splitter.DragDelta += GridSplitter_DragDelta;
			//splitter.DragCompleted += GridSplitter_DragCompleted; // bug, this is firing when double clicking splitter
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

				AddSplitters();
			}
		}

		private RenderTargetBitmap GetSelectedBitmap()
		{
			if (selectionRect.Width == 0 || selectionRect.Height == 0)
				return null;

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

			//var selectionBitmap = GetSelectedBitmap();
			Size sourceSize = correctedBitmap.Size;

			selectionBitmap = new RenderTargetBitmap(new PixelSize((int)sourceSize.Width, (int)sourceSize.Height), new Vector(96, 96));

			var borderPen = new Pen(Brushes.Red, lineCap: PenLineCap.Square);
			using (var ctx = selectionBitmap.CreateDrawingContext(null))
			{
				ctx.DrawImage(correctedBitmap.PlatformImpl, 1, selectionRect, selectionRect);
				ctx.DrawRectangle(borderPen, selectionRect);
			}
			selectionImage.Source = selectionBitmap;
			//selectionImage.Posit
		}
	}
}
