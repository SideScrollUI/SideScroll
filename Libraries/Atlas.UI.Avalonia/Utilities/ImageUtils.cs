using Atlas.Core;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace Atlas.UI.Avalonia.Utilities;

class ImageUtils
{
	public const int MaxImageWidth = 10000;
	public const int MaxImageHeight = 1024;

	public const string Base64Prefix = "data:image/png;base64,";

	public static Bitmap? LoadBitmap(Call call, byte[] bytes)
	{
		try
		{
			var stream = new MemoryStream(bytes);
			var bitmap = new Bitmap(stream);
			return bitmap;
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
		return null;
	}

	public static Bitmap? LoadImage(Call call, Image image, string path)
	{
		try
		{
			byte[] bytes = File.ReadAllBytes(path);
			if (LoadBitmap(call, bytes) is Bitmap bitmap && bitmap.Size.Width < MaxImageWidth)
			{
				image!.Source = bitmap;
				image.MaxHeight = MaxImageHeight;
				return bitmap;
			}
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
		return null;
	}

	public static string? ConvertBitmapToBase64(Call call, Bitmap? bitmap)
	{
		if (bitmap == null) return null;

		try
		{
			using var stream = new MemoryStream();
			bitmap.Save(stream);
			byte[] bytes = stream.GetBuffer();
			return Base64Prefix + Convert.ToBase64String(bytes);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
		return null;
	}
}
