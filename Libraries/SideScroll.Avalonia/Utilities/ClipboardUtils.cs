using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
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
	/// Sets text to the clipboard asynchronously
	/// </summary>
	public static void SetText(Visual? visual, string text)
	{
		Dispatcher.UIThread.Post(async () => await SetTextAsync(visual, text));
	}

	/// <summary>
	/// Sets text to the clipboard
	/// </summary>
	public static async Task SetTextAsync(Visual? visual, string text)
	{
		IClipboard clipboard = GetClipboard(visual);
		await clipboard.SetTextAsync(text);
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
}
