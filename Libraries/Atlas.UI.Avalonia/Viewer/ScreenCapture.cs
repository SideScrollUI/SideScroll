using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia
{
	public class ScreenCapture : Grid
	{
		private RenderTargetBitmap _correctedBitmap;
		private RenderTargetBitmap _selectionBitmap;
		private Image _backgroundImage;
		private Image _selectionImage;

		private Point? _startPoint;
		private Rect _selectionRect;

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
			Cursor = new Cursor(StandardCursorType.Cross);

			AddBackgroundImage(visual);

			_selectionImage = new Image()
			{
				//[Grid.ColumnProperty] = 2,
				//[Grid.RowProperty] = 2,
				[Grid.ColumnSpanProperty] = 5,
				[Grid.RowSpanProperty] = 5,
			};
			Children.Add(_selectionImage);

			//rtb.Save(Path.Combine(OutputPath, testName + ".out.png"));

			PointerPressed += ScreenCapture_PointerPressed;
			PointerReleased += ScreenCapture_PointerReleased;
			PointerMoved += ScreenCapture_PointerMoved;
		}

		private void AddBackgroundImage(IVisual visual)
		{
			// visual is offset by the Toolbar height
			var bounds = visual.Bounds;

			var sourceBitmap = new RenderTargetBitmap(new PixelSize((int)bounds.Right, (int)bounds.Bottom), new Vector(96, 96));
			sourceBitmap.Render(visual);

			_correctedBitmap = new RenderTargetBitmap(new PixelSize((int)bounds.Width, (int)bounds.Height), new Vector(96, 96));

			using (var ctx = _correctedBitmap.CreateDrawingContext(null))
			{
				var destRect = new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
				ctx.DrawBitmap(sourceBitmap.PlatformImpl, 0.75, bounds, destRect);
			}

			_backgroundImage = new Image()
			{
				Source = _correctedBitmap,
				//[Grid.RowProperty] = 1,
				[Grid.ColumnSpanProperty] = 5,
				[Grid.RowSpanProperty] = 5,
			};
			Children.Add(_backgroundImage);
		}

		private void AddSplitters()
		{
			AddSplitter(1, 0, HorizontalAlignment.Stretch, VerticalAlignment.Top);
			AddSplitter(0, 1, HorizontalAlignment.Left, VerticalAlignment.Stretch);
			AddSplitter(2, 1, HorizontalAlignment.Right, VerticalAlignment.Stretch);
			AddSplitter(1, 2, HorizontalAlignment.Stretch, VerticalAlignment.Bottom);

			/*var panel = new Panel()
			{
				Background = Brushes.Blue,
				[Grid.ColumnProperty] = 2,
				[Grid.RowProperty] = 2,
			};
			Children.Add(panel);*/

			/*var selectionBorder = new Border()
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

		private RenderTargetBitmap GetSelectedBitmap()
		{
			if (_selectionRect.Width == 0 || _selectionRect.Height == 0)
				return null;

			var destRect = new Rect(0, 0, _selectionRect.Width, _selectionRect.Height);

			var bitmap = new RenderTargetBitmap(new PixelSize((int)destRect.Width, (int)destRect.Height), new Vector(96, 96));

			using (var ctx = bitmap.CreateDrawingContext(null))
			{
				ctx.DrawBitmap(_correctedBitmap.PlatformImpl, 1, _selectionRect, destRect);
			};
			return bitmap;
		}

		private void ScreenCapture_PointerPressed(object sender, PointerPressedEventArgs e)
		{
			//if (startPoint == null)
			{
				_startPoint = e.GetPosition(_backgroundImage);
				//PointerMoved += ScreenCapture_PointerMoved;
			}
		}

		private void ScreenCapture_PointerReleased(object sender, PointerReleasedEventArgs e)
		{
			if (_startPoint == null)
				return;

			//PointerReleased -= ScreenCapture_PointerReleased;
			var bitmap = GetSelectedBitmap();
			//ClipBoardUtils.SetTextAsync(bitmap);
			//ClipboardUtils.SetBitmapAsync(bitmap);

			_startPoint = null;

			//AddSplitters();
		}

		private void ScreenCapture_PointerMoved(object sender, PointerEventArgs e)
		{
			if (_startPoint == null)
				return;

			var mousePosition = e.GetPosition(_backgroundImage);
			Size sourceSize = _correctedBitmap.Size;

			double scaleX = sourceSize.Width / _backgroundImage.Bounds.Width;
			double scaleY = sourceSize.Height / _backgroundImage.Bounds.Height;

			var scaledStartPoint = new Point(_startPoint.Value.X * scaleX, _startPoint.Value.Y * scaleY);
			var scaledEndPoint = new Point(mousePosition.X * scaleX, mousePosition.Y * scaleY);

			Point topLeft = new Point(Math.Min(scaledStartPoint.X, scaledEndPoint.X), Math.Min(scaledStartPoint.Y, scaledEndPoint.Y));
			Point bottomRight = new Point(Math.Max(scaledStartPoint.X, scaledEndPoint.X), Math.Max(scaledStartPoint.Y, scaledEndPoint.Y));

			_selectionRect = new Rect(topLeft, bottomRight);
			//var destRect = new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);

			//var selectionBitmap = GetSelectedBitmap();

			_selectionBitmap = new RenderTargetBitmap(new PixelSize((int)sourceSize.Width, (int)sourceSize.Height), new Vector(96, 96));

			var borderPen = new Pen(Brushes.Red, lineCap: PenLineCap.Square);
			using (var ctx = _selectionBitmap.CreateDrawingContext(null))
			{
				ctx.DrawBitmap(_correctedBitmap.PlatformImpl, 1, _selectionRect, _selectionRect);
				//ctx.DrawRectangle(borderPen, selectionRect);
			}
			_selectionImage.Source = _selectionBitmap;
			//selectionImage.Posit
		}
	}
}
