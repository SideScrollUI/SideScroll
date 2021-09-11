using Atlas.Core;
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
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia
{
	public class ScreenCapture : Grid
	{
		private const int MinClipboardSize = 10;

		private RenderTargetBitmap _originalBitmap;
		private RenderTargetBitmap _backgroundBitmap; // 50% faded
		private RenderTargetBitmap _selectionBitmap;

		private Grid _contentGrid;
		private Image _backgroundImage;
		private Image _selectionImage;

		private Point? _startPoint;
		private Rect _selectionRect;

		public TabViewer TabViewer;

		public ScreenCapture(TabViewer tabViewer, IVisual visual)
		{
			TabViewer = tabViewer;

			InitializeComponent(visual);
		}

		private void InitializeComponent(IVisual visual)
		{
			Background = Brushes.Black;

			HorizontalAlignment = HorizontalAlignment.Left;
			VerticalAlignment = VerticalAlignment.Top;

			ColumnDefinitions = new ColumnDefinitions("*");
			RowDefinitions = new RowDefinitions("Auto,*");

			AddToolbar();
			AddContent(visual);
		}

		private void AddToolbar()
		{
			var toolbar = new ScreenCaptureToolbar(TabViewer);
			toolbar.ButtonCopyClipboard.Add(CopyClipboard);
			toolbar.ButtonSave.AddAsync(SaveAsync);
			toolbar.ButtonClose.Add(Close);
			Children.Add(toolbar);
		}

		private void AddContent(IVisual visual)
		{
			_contentGrid = new Grid()
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Cursor = new Cursor(StandardCursorType.Cross),
				[Grid.RowProperty] = 1,
			};
			Children.Add(_contentGrid);

			AddBackgroundImage(visual);

			_selectionImage = new Image()
			{
				Stretch = Stretch.None,
			};
			_contentGrid.Children.Add(_selectionImage);

			_contentGrid.PointerPressed += ScreenCapture_PointerPressed;
			_contentGrid.PointerReleased += ScreenCapture_PointerReleased;
			_contentGrid.PointerMoved += ScreenCapture_PointerMoved;
		}

		private void CopyClipboard(Call call)
		{
			OSPlatform platform = ProcessUtils.GetOSPlatform();
			if (platform != OSPlatform.Windows)
				return;

			RenderTargetBitmap bitmap = GetSelectedBitmap();
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
		}

		private async Task SaveAsync(Call call)
		{
			RenderTargetBitmap bitmap = GetSelectedBitmap();
			if (bitmap == null)
				return;

			var fileDialog = new SaveFileDialog()
			{
				Directory = Paths.PicturesPath,
				InitialFileName = TabViewer.Project.Name + '.' + FileUtils.TimestampString + ".png",
				DefaultExtension = "png",
				Filters = new List<FileDialogFilter>()
				{
					new FileDialogFilter()
					{
						Name = "Portable Network Graphic file (PNG)",
						Extensions = new List<string>() { "png" }
					}
				},
			};
			var window = GetWindow(this);
			string filePath = await fileDialog.ShowAsync(window);
			if (filePath != null)
				bitmap.Save(filePath);
		}

		private Window GetWindow(IControl control)
		{
			if (control is Window window)
				return window;

			if (control.Parent == null)
				return null;

			return GetWindow(control.Parent);
		}

		private void Close(Call call)
		{
			TabViewer.CloseSnapshot(call);
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
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Stretch = Stretch.None,
				Source = _backgroundBitmap,
			};
			_contentGrid.Children.Add(_backgroundImage);
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

			CopyClipboard(new Call());

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

			UpdateSelectionImage();
		}

		private void UpdateSelectionImage()
		{
			Size sourceSize = _originalBitmap.Size;
			_selectionBitmap = new RenderTargetBitmap(new PixelSize((int)sourceSize.Width, (int)sourceSize.Height), new Vector(96, 96));

			var borderRect = new Rect(
				new Point(
					Math.Max(2, _selectionRect.Left),
					Math.Max(2, _selectionRect.Top)),
				_selectionRect.BottomRight);

			//var brush = new SolidColorBrush(Color.Parse("#8818ff"));
			//var brush = Theme.ToolbarTextForeground;
			var brush = Theme.ToolbarLabelForeground;
			//var brush = Brushes.White;
			var innerPen = new Pen(Brushes.Black, 2, lineCap: PenLineCap.Square);
			var outerPen = new Pen(brush, 4, lineCap: PenLineCap.Square);
			using (var ctx = _selectionBitmap.CreateDrawingContext(null))
			{
				ctx.DrawBitmap(_originalBitmap.PlatformImpl, 1, _selectionRect, _selectionRect);
				ctx.DrawRectangle(null, outerPen, borderRect.Inflate(1));
				ctx.DrawRectangle(null, innerPen, borderRect);
			}
			_selectionImage.Source = _selectionBitmap;
		}
	}
}
