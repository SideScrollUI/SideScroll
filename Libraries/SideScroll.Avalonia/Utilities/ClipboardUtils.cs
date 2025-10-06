using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace SideScroll.Avalonia.Utilities;

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

	public static void SetText(Visual? visual, string text)
	{
		Task.Run(() => SetTextAsync(visual, text));
	}

	public static async Task SetTextAsync(Visual? visual, string text)
	{
		IClipboard clipboard = GetClipboard(visual);
		await clipboard.SetTextAsync(text);
	}

	public static string? TryGetText(Visual? visual)
	{
		return Task.Run(() => TryGetTextAsync(visual)).GetAwaiter().GetResult();
	}

	public static async Task<string?> TryGetTextAsync(Visual? visual)
	{
		IClipboard? clipboard = TryGetClipboard(visual);
		if (clipboard == null) return null;

		string? clipboardText = await clipboard.TryGetTextAsync();
		return clipboardText;
	}
}
