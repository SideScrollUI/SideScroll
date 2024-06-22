using SideScroll.Resources;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.Reflection;

namespace SideScroll.UI.Avalonia;

public static class AvaloniaAssets
{
	public static Bitmap GetBitmap(string name)
	{
		Assembly assembly = Assembly.GetExecutingAssembly();

		using Stream stream = assembly.GetManifestResourceStream(name)!;
		return new Bitmap(stream);
	}

	private static Image GetImage(Bitmap bitmap)
	{
		return new Image
		{
			Source = bitmap,
		};
	}

	public static class Bitmaps
	{
		public static Bitmap Help => new(Icons.Svg.Forward.Stream);
		public static Bitmap Info => new(Icons.Png.Info.Stream);
		public static Bitmap Hourglass => new(Assets.Png.Hourglass.Stream);
		public static Bitmap Shutter => new(Assets.Png.Shutter.Stream);
		public static Bitmap Logo => new(Icons.Logo.Stream);
	}

	public static class Images
	{
		public static Image Help => GetImage(Bitmaps.Help);
		public static Image Info => GetImage(Bitmaps.Info);
	}
}
