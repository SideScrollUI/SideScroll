using System;
using System.Runtime.InteropServices;

namespace Atlas.UI.Avalonia
{
	public static class Gdi32UnmanagedMethods
	{
		private const string Library = "gdi32.dll";


		[DllImport(Library, ExactSpelling = true)]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport(Library, ExactSpelling = true)]
		public static extern bool DeleteDC(IntPtr hdc);


		[DllImport(Library, ExactSpelling = true)]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);


		[DllImport(Library, SetLastError = true, ExactSpelling = true)]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);


		[DllImport(Library, SetLastError = true, ExactSpelling = true)]
		public static extern bool BitBlt(
			IntPtr hdc,
			int x,
			int y,
			int cx,
			int cy,
			IntPtr hdcSrc,
			int x1,
			int y1,
			uint rop);

		public class DcScope : IDisposable
		{
			public IntPtr HDC;

			public DcScope(IntPtr hdc)
			{
				HDC = CreateCompatibleDC(hdc);
			}

			public static implicit operator IntPtr(DcScope dcScope) => dcScope.HDC;

			public void Dispose()
			{
				if (HDC != IntPtr.Zero)
				{
					DeleteDC(HDC);
					HDC = IntPtr.Zero;
				}
			}
		}

		public static DcScope CreateCompatibleDCScoped(IntPtr hdc)
		{
			return new DcScope(hdc);
		}
	}
}
