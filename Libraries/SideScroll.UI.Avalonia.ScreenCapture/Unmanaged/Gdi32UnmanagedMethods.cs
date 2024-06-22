using System.Runtime.InteropServices;

namespace SideScroll.UI.Avalonia.ScreenCapture.Unmanaged;

public static class Gdi32UnmanagedMethods
{
	private const string Library = "gdi32.dll";


	[DllImport(Library, ExactSpelling = true)]
	internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

	[DllImport(Library, ExactSpelling = true)]
	internal static extern bool DeleteDC(IntPtr hdc);


	[DllImport(Library, ExactSpelling = true)]
	internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);


	[DllImport(Library, SetLastError = true, ExactSpelling = true)]
	internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);


	[DllImport(Library, SetLastError = true, ExactSpelling = true)]
	internal static extern bool BitBlt(
		IntPtr hdc,
		int x,
		int y,
		int cx,
		int cy,
		IntPtr hdcSrc,
		int x1,
		int y1,
		uint rop);

	public class DcScope(IntPtr hdc) : IDisposable
	{
		public IntPtr HDC = CreateCompatibleDC(hdc);

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
