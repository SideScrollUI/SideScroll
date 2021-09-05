using Atlas.UI.Avalonia.Utilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia
{
	public class ScreenCapture : Grid
	{
		private const int MinClipboardSize = 10;

		private RenderTargetBitmap _originalBitmap;
		private RenderTargetBitmap _backgroundBitmap; // 50% faded
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

			HorizontalAlignment = HorizontalAlignment.Left;
			VerticalAlignment = VerticalAlignment.Top;

			ColumnDefinitions = new ColumnDefinitions("*");
			RowDefinitions = new RowDefinitions("*");

			Cursor = new Cursor(StandardCursorType.Cross);

			AddBackgroundImage(visual);

			_selectionImage = new Image();
			Children.Add(_selectionImage);

			PointerPressed += ScreenCapture_PointerPressed;
			PointerReleased += ScreenCapture_PointerReleased;
			PointerMoved += ScreenCapture_PointerMoved;
		}

		private void AddBackgroundImage(IVisual visual)
		{
			var bounds = visual.Bounds;

			_originalBitmap = new RenderTargetBitmap(new PixelSize((int)bounds.Right, (int)bounds.Bottom), new Vector(96, 96));
			_originalBitmap.Render(visual);

			_backgroundBitmap = new RenderTargetBitmap(new PixelSize((int)bounds.Width, (int)bounds.Height), new Vector(96, 96));

			using (var ctx = _backgroundBitmap.CreateDrawingContext(null))
			{
				ctx.DrawBitmap(_originalBitmap.PlatformImpl, 0.5, bounds, bounds);
			}

			_backgroundImage = new Image()
			{
				Source = _backgroundBitmap,
			};
			Children.Add(_backgroundImage);
		}

		private RenderTargetBitmap GetSelectedBitmap()
		{
			if (_selectionRect.Width < MinClipboardSize || _selectionRect.Height < MinClipboardSize)
				return null;

			var destRect = new Rect(0, 0, _selectionRect.Width, _selectionRect.Height);

			var bitmap = new RenderTargetBitmap(new PixelSize((int)destRect.Width, (int)destRect.Height), new Vector(96, 96));

			using (var ctx = bitmap.CreateDrawingContext(null))
			{
				ctx.DrawBitmap(_originalBitmap.PlatformImpl, 1, _selectionRect, destRect);
			};
			return bitmap;
		}

		private void ScreenCapture_PointerPressed(object sender, PointerPressedEventArgs e)
		{
			_startPoint = e.GetPosition(_backgroundImage);
		}

		private void ScreenCapture_PointerReleased(object sender, PointerReleasedEventArgs e)
		{
			if (_startPoint == null)
				return;

			var bitmap = GetSelectedBitmap();
			if (bitmap == null)
				return;

			//ClipBoardUtils.SetTextAsync(bitmap); // AvaloniaUI will probably eventually support this
			try
			{
				using (bitmap)
				{
					Task.Run(() => Win32ClipboardUtils.SetBitmapAsync(bitmap)).GetAwaiter().GetResult();
				}
			}
			catch
			{

			}

			_startPoint = null;
		}

		private void ScreenCapture_PointerMoved(object sender, PointerEventArgs e)
		{
			if (_startPoint == null)
				return;

			Point mousePosition = e.GetPosition(_backgroundImage);
			Size sourceSize = _originalBitmap.Size;

			double scaleX = sourceSize.Width / _backgroundImage.Bounds.Width;
			double scaleY = sourceSize.Height / _backgroundImage.Bounds.Height;

			double startX = Math.Max(0, _startPoint.Value.X);
			double startY = Math.Max(0, _startPoint.Value.Y);

			double endX = Math.Max(0, mousePosition.X);
			double endY = Math.Max(0, mousePosition.Y);

			var scaledStartPoint = new Point(startX * scaleX, startY * scaleY);
			var scaledEndPoint = new Point(endX * scaleX, endY * scaleY);

			var topLeft = new Point(
				Math.Min(scaledStartPoint.X, scaledEndPoint.X), 
				Math.Min(scaledStartPoint.Y, scaledEndPoint.Y));

			var bottomRight = new Point(
				Math.Max(scaledStartPoint.X, scaledEndPoint.X), 
				Math.Max(scaledStartPoint.Y, scaledEndPoint.Y));

			_selectionRect = new Rect(topLeft, bottomRight);

			_selectionBitmap = new RenderTargetBitmap(new PixelSize((int)sourceSize.Width, (int)sourceSize.Height), new Vector(96, 96));

			var borderRect = new Rect(topLeft, bottomRight).Inflate(2);

			//var brush = new SolidColorBrush(Color.Parse("#8818ff"));
			//var brush = Theme.GridBackgroundSelected;
			var brush = Brushes.Red;
			var borderPen = new Pen(brush, 2, lineCap: PenLineCap.Square);
			var borderBlackPen = new Pen(Brushes.Black, 4, lineCap: PenLineCap.Square);
			using (var ctx = _selectionBitmap.CreateDrawingContext(null))
			{
				ctx.DrawBitmap(_originalBitmap.PlatformImpl, 1, _selectionRect, _selectionRect);
				ctx.DrawRectangle(null, borderBlackPen, borderRect);
				ctx.DrawRectangle(null, borderPen, borderRect);
			}
			_selectionImage.Source = _selectionBitmap;
		}
	}
}
