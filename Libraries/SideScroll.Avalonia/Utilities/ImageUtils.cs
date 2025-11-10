using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace SideScroll.Avalonia.Utilities;

/// <summary>
/// Provides utility methods for loading and processing images in Avalonia
/// </summary>
public static class ImageUtils
{
	/// <summary>
	/// Gets or sets the maximum allowed image dimension (width or height) in pixels
	/// </summary>
	public static int MaxImageSize { get; set; } = 10_000;

	/// <summary>
	/// Loads a bitmap from byte array data
	/// </summary>
	public static Bitmap LoadBitmap(byte[] bytes)
	{
		var stream = new MemoryStream(bytes);
		return new Bitmap(stream);
	}

	/// <summary>
	/// Loads an image from a file path and sets it as the source for an Image control.
	/// Validates that image dimensions do not exceed MaxImageSize.
	/// </summary>
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
