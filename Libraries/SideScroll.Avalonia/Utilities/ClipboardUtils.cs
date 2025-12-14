using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace SideScroll.Avalonia.Utilities;

/// <summary>
/// Provides utility methods for clipboard operations in Avalonia
/// </summary>
public static class ClipboardUtils
{
	private static IClipboard GetClipboard(Visual? visual)
	{
		return (TopLevel.GetTopLevel(visual)?.Clipboard)
					?? throw new Exception("Failed to get clipboard");
	}

	private static IClipboard? TryGetClipboard(Visual? visual)
	{
		return TopLevel.GetTopLevel(visual)?.Clipboard;
	}

	/// <summary>
	/// Sets clipboard text asynchronously on the UI Thread
	/// </summary>
	public static void SetText(Visual? visual, string text)
	{
		Dispatcher.UIThread.Post(async () => await SetTextAsync(visual, text));
	}

	/// <summary>
	/// Sets clipboard to text
	/// </summary>
	public static async Task SetTextAsync(Visual? visual, string text)
	{
		IClipboard clipboard = GetClipboard(visual);
		await clipboard.SetTextAsync(text);
		await clipboard.FlushAsync(); // Flush to retain clipboard content after app closes
	}

	/// <summary>
	/// Attempts to retrieve text from the clipboard
	/// </summary>
	public static async Task<string?> TryGetTextAsync(Visual? visual)
	{
		IClipboard? clipboard = TryGetClipboard(visual);
		if (clipboard == null) return null;

		string? clipboardText = await clipboard.TryGetTextAsync();
		return clipboardText;
	}

	/// <summary>
	/// Sets clipboard to bitmap
	/// </summary>
	public static async Task SetBitmapAsync(Visual? visual, Bitmap? bitmap)
	{
		IClipboard clipboard = GetClipboard(visual);
		await clipboard.SetBitmapAsync(bitmap);
		await clipboard.FlushAsync(); // Flush to retain clipboard content after app closes
	}
}
