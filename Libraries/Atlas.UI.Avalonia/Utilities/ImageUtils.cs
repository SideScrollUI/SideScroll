using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace Atlas.UI.Avalonia.Utilities;

public static class ImageUtils
{
	public const int MaxImageSize = 10_000;

	public static Bitmap LoadBitmap(byte[] bytes)
	{
		var stream = new MemoryStream(bytes);
		return new Bitmap(stream);
	}

	public static Bitmap LoadImage(Image image, string path)
	{
		byte[] bytes = File.ReadAllBytes(path);
		Bitmap bitmap = LoadBitmap(bytes);
		if (bitmap.Size.Width > MaxImageSize) throw new Exception($"Image width {bitmap.Size.Width} is above maximum {MaxImageSize}");
		if (bitmap.Size.Height > MaxImageSize) throw new Exception($"Image height {bitmap.Size.Height} is above maximum {MaxImageSize}");

		image.Source = bitmap;
		return bitmap;
	}
}
