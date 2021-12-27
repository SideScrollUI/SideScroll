using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia.ScreenCapture
{
	public static class Win32ClipboardUtils
	{
		private const int SRCCOPY = 0x00CC0020;

		private static async Task OpenClipboard()
		{
			while (!Win32UnmanagedMethods.OpenClipboard(IntPtr.Zero))
			{
				await Task.Delay(100);
			}
		}

		public static async Task SetBitmapAsync(Bitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException(nameof(bitmap));

			// Convert from Avalonia Bitmap to System Bitmap
			using var memoryStream = new MemoryStream(1000000);

			bitmap.Save(memoryStream); // this returns a png from Skia

			using var systemBitmap = new System.Drawing.Bitmap(memoryStream);

			await SetBitmapAsync(systemBitmap);
		}

		private static async Task SetBitmapAsync(System.Drawing.Bitmap systemBitmap)
		{
			//var bitmapStream = new MemoryStream();
			//systemBitmapPng.Save(bitmapStream, ImageFormat.Bmp);
			//var systemBitmap = new System.Drawing.Bitmap(bitmapStream); // .bmp version imports clearer than .png

			var hBitmap = systemBitmap.GetHbitmap();

			var screenDC = Win32UnmanagedMethods.GetDC(IntPtr.Zero);

			using var sourceDC = Gdi32UnmanagedMethods.CreateCompatibleDCScoped(screenDC);

			if (sourceDC.HDC == IntPtr.Zero)
				return;

			var sourceBitmapSelection = Gdi32UnmanagedMethods.SelectObject(sourceDC, hBitmap);
			if (sourceBitmapSelection == null)
				return;

			using var destDC = Gdi32UnmanagedMethods.CreateCompatibleDCScoped(screenDC);

			if (destDC.HDC == IntPtr.Zero)
				return;

			var compatibleBitmap = Gdi32UnmanagedMethods.CreateCompatibleBitmap(screenDC, systemBitmap.Width, systemBitmap.Height);
			if (compatibleBitmap == IntPtr.Zero)
				return;

			var destinationBitmapSelection = Gdi32UnmanagedMethods.SelectObject(destDC, compatibleBitmap);
			if (destinationBitmapSelection == null)
				return;

			if (!Gdi32UnmanagedMethods.BitBlt(
				destDC,
				0,
				0,
				systemBitmap.Width,
				systemBitmap.Height,
				sourceDC,
				0,
				0,
				SRCCOPY))
			{
				throw new Exception("BitBlt failed");
			}

			try
			{
				await OpenClipboard();

				Win32UnmanagedMethods.EmptyClipboard();

				IntPtr result = Win32UnmanagedMethods.SetClipboardData(Win32UnmanagedMethods.ClipboardFormat.CF_BITMAP, compatibleBitmap);

				if (result == IntPtr.Zero)
				{
					int errno = Marshal.GetLastWin32Error();
					throw new Exception("SetClipboardData failed: Error = " + errno);
				}
			}
			finally
			{
				Win32UnmanagedMethods.CloseClipboard();
			}
		}
	}
}
