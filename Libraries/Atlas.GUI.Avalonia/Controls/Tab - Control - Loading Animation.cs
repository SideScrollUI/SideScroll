using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlLoadingAnimation : Control
	{
		private Bitmap source = AvaloniaAssets.Bitmaps.Shutter;
		private RenderTargetBitmap _bitmap;

		public TabControlLoadingAnimation()
		{
			Width = Height = source.Size.Width;
		}

		protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
		{
			_bitmap = new RenderTargetBitmap(new PixelSize(200, 200), new Vector(96, 96));
			base.OnAttachedToLogicalTree(e);
		}

		protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
		{
			_bitmap.Dispose();
			_bitmap = null;
			base.OnDetachedFromLogicalTree(e);
		}

		readonly Stopwatch _st = Stopwatch.StartNew();
		/*public override void Render(DrawingContext context)
		{
			using (var ctxi = _bitmap.CreateDrawingContext(null))
			using (var ctx = new DrawingContext(ctxi, false))
			using (ctx.PushPostTransform(Matrix.CreateTranslation(-100, -100)
										 * Matrix.CreateRotation(_st.Elapsed.TotalSeconds)
										 * Matrix.CreateTranslation(100, 100)))
			{
				ctxi.Clear(default);
				ctx.FillRectangle(Brush.Parse("#006df0"), new Rect(50, 50, 100, 100));
			}

			context.DrawImage(_bitmap, 1,
				new Rect(0, 0, 200, 200),
				new Rect(0, 0, 200, 200));
			Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
			base.Render(context);
		}*/

		public override void Render(DrawingContext context)
		{
			int width = (int)source.Size.Width;
			int halfWidth = width / 2;
			using (var ctxi = _bitmap.CreateDrawingContext(null))
			using (var ctx = new DrawingContext(ctxi, false))
			using (ctx.PushPostTransform(Matrix.CreateTranslation(-halfWidth, -halfWidth)
										 * Matrix.CreateRotation(_st.Elapsed.TotalSeconds)
										 * Matrix.CreateTranslation(halfWidth, halfWidth)))
			{
				ctxi.Clear(default);
				ctx.DrawImage(source, 1, new Rect(source.Size), new Rect(source.Size));
				//ctx.FillRectangle(Brush.Parse("#006df0"), new Rect(50, 50, 100, 100));
			}

			context.DrawImage(_bitmap, 1,
				new Rect(0, 0, width, width),
				new Rect(0, 0, width, width));
			Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
			base.Render(context);
		}
	}
}
/*
From Avalonia RenderDemo RenderTargetBitmapPage
*/
