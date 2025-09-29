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

	public static void SetText(Visual? visual, string text)
	{
		Task.Run(() => SetTextAsync(visual, text));
	}

	public static async Task SetTextAsync(Visual? visual, string text)
	{
		IClipboard clipboard = GetClipboard(visual);
		await clipboard.SetTextAsync(text);
	}

	public static string? GetText(Visual? visual)
	{
		return Task.Run(() => GetTextAsync(visual)).GetAwaiter().GetResult();
	}

	public static async Task<string?> GetTextAsync(Visual? visual)
	{
		IClipboard clipboard = GetClipboard(visual);
		string? clipboardText = await clipboard.GetTextAsync();
		return clipboardText;
	}
}
