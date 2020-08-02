using Avalonia;
using Avalonia.Input.Platform;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia
{
	public class ClipBoardUtils
	{
		public static async Task SetTextAsync(string text)
		{
			await ((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(text);
		}

		public static async Task<string> GetTextAsync()
		{
			string clipboardText = await ((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).GetTextAsync();
			return clipboardText;
		}
	}
}
