using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls.View;
using SideScroll.Tabs;
using System.Diagnostics;

namespace SideScroll.Avalonia.Controls.Viewer;

public static class AvaloniaHeadlessCapture
{
	/// <summary>
	/// Loads <paramref name="tab"/> into a headless <see cref="TabViewer"/> and returns the
	/// <see cref="Window"/> after all tabs up to <paramref name="maxTabDepth"/> have been rendered.
	/// The window is sized to <paramref name="maxWidth"/> × <paramref name="maxHeight"/>; use
	/// <see cref="GetCaptureRect"/> to obtain the exact content rectangle and crop accordingly.
	/// Call <c>window.CaptureRenderedFrame()</c> (from <c>Avalonia.Headless</c>) on the returned
	/// window to produce the bitmap.
	/// </summary>
	/// <param name="project">The project used to initialise the viewer.</param>
	/// <param name="tab">The root tab to load.</param>
	/// <param name="maxTabDepth">
	/// How many levels of tabs to render, where the root is depth 1.
	/// Tabs at this depth show their own content but do not create child tabs.
	/// Pass <c>null</c> for no limit.
	/// </param>
	/// <param name="maxWidth">Maximum window width in pixels.</param>
	/// <param name="maxHeight">Maximum window height in pixels.</param>
	/// <param name="timeoutMs">Maximum milliseconds to wait for loading before returning anyway.</param>
	public static Window RenderTab(
		Project project,
		ITab tab,
		int? maxTabDepth = 1,
		int maxWidth = 1600,
		int maxHeight = 900,
		int timeoutMs = 10_000)
	{
		var window = new Window
		{
			Width = maxWidth,
			Height = maxHeight,
			SystemDecorations = SystemDecorations.None,
		};

		var viewer = new TabViewer(project, isWindowed: false)
		{
			MaxTabDepth = maxTabDepth,
		};

		window.Content = viewer;
		window.Show();
		Dispatcher.UIThread.RunJobs();

		viewer.LoadTab(tab);

		// Alternate between sleeping (so background data-load threads can complete and post
		// back to the dispatcher) and draining the dispatcher queue (so timer ticks fire and
		// posted UI work runs). This must stay on the main/UI thread throughout.
		var sw = Stopwatch.StartNew();
		while (!viewer.ChildrenLoadedAsync.IsCompleted && sw.ElapsedMilliseconds < timeoutMs)
		{
			Thread.Sleep(15);
			Dispatcher.UIThread.RunJobs();
		}

		Dispatcher.UIThread.RunJobs();
		return window;
	}

	/// <summary>
	/// Returns the pixel rectangle (relative to the window) that covers the tab columns from
	/// <paramref name="minTabDepth"/> through <paramref name="maxTabDepth"/> (inclusive).
	/// The toolbar, side-scroll buttons, and any columns outside the specified range are
	/// automatically excluded.
	/// <para>
	/// Depth numbering matches <see cref="TabInstance.Depth"/>: the root tab loaded by
	/// <see cref="TabViewer.LoadTab"/> is depth 1.
	/// </para>
	/// </summary>
	/// <param name="viewer">The viewer returned inside the window from <see cref="RenderTab"/>.</param>
	/// <param name="minTabDepth">First column to include (1 = leftmost).</param>
	/// <param name="maxTabDepth">Last column to include. <c>null</c> means the deepest rendered column.</param>
	public static Rect GetCaptureRect(TabViewer viewer, int minTabDepth = 1, int? maxTabDepth = null)
	{
		if (viewer.TabView == null)
		{
			return default;
		}

		// Build the left-to-right chain of TabViews (depth 1, 2, 3, …)
		var chain = new List<TabView>();
		var tv = viewer.TabView;
		while (tv != null)
		{
			chain.Add(tv);
			tv = tv.Instance.ChildTabInstances.Keys.OfType<TabView>().FirstOrDefault();
		}

		int minIdx = Math.Max(0, minTabDepth - 1);
		int maxIdx = maxTabDepth.HasValue
			? Math.Min(maxTabDepth.Value - 1, chain.Count - 1)
			: chain.Count - 1;

		if (minIdx >= chain.Count)
		{
			return default;
		}

		// Left edge: translated position of the first desired column
		Point? left = chain[minIdx].TranslatePoint(default(Point), viewer);

		// Right edge: content-panel width + GridSplitter width for the last included column.
		// Using ColumnBoundaryWidth excludes the filler panel that the deepest column adds to
		// give the splitter drag room (which would otherwise appear as blank space on the right).
		Point? right = chain[maxIdx].TranslatePoint(
			new Point(chain[maxIdx].ColumnBoundaryWidth, 0), viewer);

		// Top/height: use the root TabView's translated position and its natural (desired) height
		// so that toolbar rows and bottom padding are excluded regardless of window stretch.
		Point? top = chain[0].TranslatePoint(default(Point), viewer);
		double height = chain[0].DesiredSize.Height;

		if (left == null || right == null || top == null)
		{
			return default;
		}

		return new Rect(left.Value.X, top.Value.Y, right.Value.X - left.Value.X, height);
	}

	/// <summary>
	/// Convenience wrapper that renders <paramref name="tab"/>, captures a frame using
	/// <paramref name="captureFrame"/>, and returns a bitmap cropped to the specified depth range.
	/// <para>
	/// The <paramref name="captureFrame"/> delegate exists so that the caller (which must reference
	/// <c>Avalonia.Headless</c>) can supply <c>window.CaptureRenderedFrame()</c> without this
	/// library taking a hard dependency on that package.
	/// </para>
	/// </summary>
	/// <param name="project">The project used to initialise the viewer.</param>
	/// <param name="tab">The root tab to load.</param>
	/// <param name="captureFrame">Called with the rendered window to produce the raw full-size bitmap.</param>
	/// <param name="minTabDepth">First column to include in the crop (1 = leftmost).</param>
	/// <param name="maxTabDepth">Last column to include. <c>null</c> means the deepest rendered column.</param>
	/// <param name="maxTabDepthRender">How many levels to render. Defaults to <paramref name="maxTabDepth"/>.</param>
	/// <param name="maxWidth">Maximum window width in pixels.</param>
	/// <param name="maxHeight">Maximum window height in pixels.</param>
	/// <param name="timeoutMs">Maximum milliseconds to wait for loading.</param>
	public static Bitmap RenderAndCrop(
		Project project,
		ITab tab,
		Func<Window, WriteableBitmap?> captureFrame,
		int minTabDepth = 1,
		int? maxTabDepth = null,
		int? maxTabDepthRender = null,
		int maxWidth = 1600,
		int maxHeight = 900,
		int timeoutMs = 10_000)
	{
		int? renderDepth = maxTabDepthRender ?? maxTabDepth;

		Window window = RenderTab(project, tab, renderDepth, maxWidth, maxHeight, timeoutMs);

		var viewer = (TabViewer)window.Content!;
		Rect captureRect = GetCaptureRect(viewer, minTabDepth, maxTabDepth);

		WriteableBitmap? full = captureFrame(window);
		return CropToCaptureRect(full!, captureRect);
	}

	/// <summary>Crops <paramref name="source"/> to <paramref name="rect"/>, clamped to the bitmap's actual pixel dimensions.</summary>
	public static Bitmap CropToCaptureRect(WriteableBitmap source, Rect rect)
	{
		int x = (int)Math.Floor(rect.X);
		int y = (int)Math.Floor(rect.Y);
		int w = (int)Math.Min(Math.Ceiling(rect.Width), source.PixelSize.Width - x);
		int h = (int)Math.Min(Math.Ceiling(rect.Height), source.PixelSize.Height - y);

		var rtb = new RenderTargetBitmap(new PixelSize(w, h), new Vector(96, 96));
		using (var ctx = rtb.CreateDrawingContext())
		{
			ctx.DrawImage(source, new Rect(x, y, w, h), new Rect(0, 0, w, h));
		}
		return rtb;
	}
}
