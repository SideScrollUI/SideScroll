using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Diagnostics;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlLoadingAnimation : Control
	{
		private const int maxWidth = 100;

		private Bitmap source = AvaloniaAssets.Bitmaps.Logo;
		//private Bitmap source = AvaloniaAssets.Bitmaps.Shutter;
		private RenderTargetBitmap _bitmap;

		public TabControlLoadingAnimation()
		{
			Width = Height = Math.Min(source.Size.Width, maxWidth);
		}

		protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
		{
			_bitmap = new RenderTargetBitmap(new PixelSize(maxWidth, maxWidth), new Vector(96, 96));
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
			int sourceWidth = (int)source.Size.Width;
			int sourceHeight = (int)source.Size.Height;
			double minDimension = Math.Min(sourceWidth, sourceHeight);
			double maxDimension = Math.Max(sourceWidth, sourceHeight);
			double sourceRadius = maxDimension / 2;

			using (var ctxi = _bitmap.CreateDrawingContext(null))
			using (var ctx = new DrawingContext(ctxi, false))
			using (ctx.PushPostTransform(Matrix.CreateTranslation(-sourceRadius, -sourceRadius)
										* Matrix.CreateRotation(_st.Elapsed.TotalSeconds)
										* Matrix.CreateTranslation(sourceRadius, sourceRadius)
										* Matrix.CreateScale(Width / sourceWidth, Height / sourceHeight)))
			{
				ctxi.Clear(default);
				ctx.DrawImage(source, 1, new Rect(source.Size), new Rect(source.Size));
				//ctx.FillRectangle(Brush.Parse("#006df0"), new Rect(50, 50, 100, 100));
			}

			context.DrawImage(_bitmap, 1,
				new Rect(0, 0, sourceWidth, sourceHeight),
				new Rect(0, 0, maxWidth, maxWidth));
			Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
			base.Render(context);
		}
	}
}
/*
From Avalonia RenderDemo RenderTargetBitmapPage
*/
