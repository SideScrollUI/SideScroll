using Avalonia;
using Avalonia.Controls;

namespace Atlas.UI.Avalonia.Utilities;

public static class ClipboardUtils
{
	public static void SetText(Visual? visual, string text)
	{
		Task.Run(() => SetTextAsync(visual, text));
	}

	public static async Task SetTextAsync(Visual? visual, string text)
	{
		var clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
		if (clipboard == null) throw new Exception("Failed to get clipboard");

		await clipboard.SetTextAsync(text);
	}

	public static string? GetText(Visual? visual)
	{
		return Task.Run(() => GetTextAsync(visual)).GetAwaiter().GetResult();
	}

	public static async Task<string?> GetTextAsync(Visual? visual)
	{
		var clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
		if (clipboard == null) throw new Exception("Failed to get clipboard");

		string? clipboardText = await clipboard.GetTextAsync();
		return clipboardText;
	}
}
