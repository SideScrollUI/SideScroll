using Atlas.Resources;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.IO;
using System.Reflection;

namespace Atlas.UI.Avalonia;

public class AvaloniaAssets
{
	public static Bitmap GetBitmap(string name)
	{
		var assembly = Assembly.GetExecutingAssembly();
		var resourceName = name;

		using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
		return new Bitmap(stream);
	}

	public static Image GetImage(Bitmap bitmap)
	{
		var image = new Image()
		{
			Source = bitmap,
		};
		return image;
	}

	public class Bitmaps
	{
		public static Bitmap Help => new(Icons.Streams.Forward);
		public static Bitmap Info => new(Icons.Streams.Info);
		public static Bitmap Hourglass => new(Assets.Streams.Hourglass);
		public static Bitmap Shutter => new(Assets.Streams.Shutter);
		public static Bitmap Logo => new(Icons.Streams.Logo);
	}

	public class Images
	{
		public static Image Help => GetImage(Bitmaps.Help);
		public static Image Info => GetImage(Bitmaps.Info);
	}
}
