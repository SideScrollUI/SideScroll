using System.Runtime.InteropServices;

namespace Atlas.UI.Avalonia.ScreenCapture.Unmanaged;

public static class Win32UnmanagedMethods
{
	private const string Library = "user32.dll";

	public enum ClipboardFormat
	{
		CF_TEXT = 1,
		CF_BITMAP = 2,
		CF_DIB = 3,
		CF_UNICODETEXT = 13,
		CF_HDROP = 15,
	}

	[DllImport(Library, SetLastError = true)]
	internal static extern bool OpenClipboard(IntPtr hWndOwner);

	[DllImport(Library, SetLastError = true)]
	internal static extern bool CloseClipboard();

	[DllImport(Library)]
	internal static extern bool EmptyClipboard();

	[DllImport(Library)]
	internal static extern IntPtr SetClipboardData(ClipboardFormat uFormat, IntPtr hMem);

	[DllImport(Library, ExactSpelling = true)]
	internal static extern IntPtr GetDC(IntPtr hWnd);
}
