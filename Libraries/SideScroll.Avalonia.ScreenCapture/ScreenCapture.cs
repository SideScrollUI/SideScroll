using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.ScreenCapture.Unmanaged;
using SideScroll.Avalonia.Themes;
using SideScroll.Resources;
using SideScroll.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SideScroll.Avalonia.ScreenCapture;

public class ScreenCapture : Grid
{
	public static int MinClipboardSize { get; set; } = 10;

	private RenderTargetBitmap? _originalBitmap;
	private RenderTargetBitmap? _backgroundBitmap; // 50% faded
	private RenderTargetBitmap? _selectionBitmap;

	private Grid? _contentGrid;
	private Image? _backgroundImage;
	private Image? _selectionImage;

	private Point? _startPoint;
	private Rect _selectionRect;

	public TabViewer TabViewer { get; }

	public class TabViewerPlugin : ITabViewerPlugin
	{
		public void Initialize(TabViewer tabViewer)
		{
			AddControlTo(tabViewer);
		}
	}

	public static ToolbarButton AddControlTo(TabViewer tabViewer)
	{
		tabViewer.Toolbar!.AddSeparator();

		ToolbarButton snapshotButton = tabViewer.Toolbar.AddButton("Snapshot", Icons.Svg.Screenshot);
		snapshotButton.Click += (s, e) =>
		{
			var screenCapture = new ScreenCapture(tabViewer, tabViewer.ScrollViewer);
			tabViewer.SetContent(screenCapture);
		};
		return snapshotButton;
	}

	public ScreenCapture(TabViewer tabViewer, Control control)
	{
		TabViewer = tabViewer;

		InitializeComponent(control);
	}

	private void InitializeComponent(Control control)
	{
		Background = Brushes.Black;

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("Auto,*");

		AddToolbar();
		AddContent(control);
	}

	private void AddToolbar()
	{
		var toolbar = new ScreenCaptureToolbar(TabViewer);
		toolbar.ButtonCopyClipboard?.Add(CopyClipboard);
		toolbar.ButtonSave.AddAsync(SaveAsync);
		toolbar.ButtonClose.Add(Close);
		Children.Add(toolbar);
	}

	private void AddContent(Control control)
	{
		_contentGrid = new Grid
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
			Cursor = new Cursor(StandardCursorType.Cross),
			[Grid.RowProperty] = 1,
		};
		Children.Add(_contentGrid);

		AddBackgroundImage(control);

		_selectionImage = new Image
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
		RenderTargetBitmap? bitmap = GetSelectedBitmap();
		if (bitmap == null)
			return;

		//ClipboardUtils.SetTextAsync(bitmap); // AvaloniaUI will probably eventually support this
		try
		{
			using (bitmap)
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					CopyClipboardWindows(bitmap);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					CopyClipboardOsx(bitmap);
				}
			}
		}
		catch (Exception e)
		{
			Debug.WriteLine(e);
		}
	}

	[SupportedOSPlatform("windows")]
	private static void CopyClipboardWindows(RenderTargetBitmap bitmap)
	{
		Task.Run(() => Win32ClipboardUtils.SetBitmapAsync(bitmap)).GetAwaiter().GetResult();
	}

	private void CopyClipboardOsx(RenderTargetBitmap bitmap)
	{
		string directory = TabViewer.Project.ProjectSettings.DefaultLocalDataPath;
		string filePath = Paths.Combine(directory, "clipboard.png");

		Directory.CreateDirectory(directory);

		bitmap.Save(filePath);

		ProcessStartInfo processStartInfo = new()
		{
			FileName = "osascript",
			ArgumentList =
			{
				"-e",
				$"set the clipboard to (read \"{filePath}\" as TIFF picture)",
			},
		};
		Process.Start(processStartInfo);
	}

	private async Task SaveAsync(Call call)
	{
		RenderTargetBitmap? bitmap = GetSelectedBitmap();
		if (bitmap == null) return;

		Window? window = GetWindow(this);
		if (window == null) return;

		var folder = await window.StorageProvider.TryGetFolderFromPathAsync(Paths.PicturesPath);

		var result = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			SuggestedStartLocation = folder,
			SuggestedFileName = $"{TabViewer.Project.Name}.{FileUtils.TimestampString}.png",
			FileTypeChoices = [FilePickerFileTypes.ImagePng],
		});
		if (result?.TryGetLocalPath() is string path)
		{
			bitmap.Save(path);
		}
	}

	private static Window? GetWindow(StyledElement styledElement)
	{
		if (styledElement is Window window)
			return window;

		if (styledElement.Parent == null)
			return null;

		return GetWindow(styledElement.Parent);
	}

	private void Close(Call call)
	{
		TabViewer.ClearContent();
	}

	private void AddBackgroundImage(Control control)
	{
		var bounds = control.Bounds;

		_originalBitmap = new RenderTargetBitmap(new PixelSize((int)bounds.Right, (int)bounds.Bottom), new Vector(96, 96));
		_originalBitmap.Render(control);

		_backgroundBitmap = new RenderTargetBitmap(new PixelSize((int)bounds.Width, (int)bounds.Height), new Vector(96, 96));

		using (var ctx = _backgroundBitmap.CreateDrawingContext())
		{
			ctx.PushOpacity(0.5);
			ctx.DrawImage(_originalBitmap, bounds);
		}

		_backgroundImage = new Image
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
			Stretch = Stretch.None,
			Source = _backgroundBitmap,
		};
		_contentGrid!.Children.Add(_backgroundImage);
	}

	private RenderTargetBitmap? GetSelectedBitmap()
	{
		if (_selectionRect.Width < MinClipboardSize || _selectionRect.Height < MinClipboardSize)
			return null;

		var destRect = new Rect(0, 0, _selectionRect.Width, _selectionRect.Height);

		var bitmap = new RenderTargetBitmap(new PixelSize((int)destRect.Width, (int)destRect.Height), new Vector(96, 96));

		using var ctx = bitmap.CreateDrawingContext();
		ctx.DrawImage(_originalBitmap!, _selectionRect, destRect);
		return bitmap;
	}

	private void ScreenCapture_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		_startPoint = e.GetPosition(_backgroundImage);
	}

	private void ScreenCapture_PointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		if (_startPoint == null)
			return;

		CopyClipboard(new Call());

		_startPoint = null;
	}

	private void ScreenCapture_PointerMoved(object? sender, PointerEventArgs e)
	{
		if (_startPoint == null)
			return;

		Point mousePosition = e.GetPosition(_backgroundImage);
		Size sourceSize = _originalBitmap!.Size;

		double scaleX = sourceSize.Width / _backgroundImage!.Bounds.Width;
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
		Size sourceSize = _originalBitmap!.Size;
		_selectionBitmap = new RenderTargetBitmap(new PixelSize((int)sourceSize.Width, (int)sourceSize.Height), new Vector(96, 96));

		var borderRect = new Rect(
			new Point(
				Math.Max(2, _selectionRect.Left),
				Math.Max(2, _selectionRect.Top)),
			_selectionRect.BottomRight);

		var brush = SideScrollTheme.ToolbarLabelForeground;
		var innerPen = new Pen(Brushes.Black, 2, lineCap: PenLineCap.Square);
		var outerPen = new Pen(brush, 4, lineCap: PenLineCap.Square);
		using (var ctx = _selectionBitmap.CreateDrawingContext())
		{
			ctx.DrawImage(_originalBitmap, _selectionRect, _selectionRect);
			ctx.DrawRectangle(null, outerPen, borderRect.Inflate(1));
			ctx.DrawRectangle(null, innerPen, borderRect);
		}
		_selectionImage!.Source = _selectionBitmap;
	}
}
