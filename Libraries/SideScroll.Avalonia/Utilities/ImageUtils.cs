using Avalonia;
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

		// Bitmap holds native memory, and an oversized image is exactly the one worth releasing
		if (bitmap.Size.Width > MaxImageSize || bitmap.Size.Height > MaxImageSize)
		{
			Size size = bitmap.Size;
			bitmap.Dispose();

			throw new Exception(size.Width > MaxImageSize
				? $"Image width {size.Width} is above maximum {MaxImageSize}"
				: $"Image height {size.Height} is above maximum {MaxImageSize}");
		}

		image.Source = bitmap;
		return bitmap;
	}
}
