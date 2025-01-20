using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SideScroll.Resources;
using System.Reflection;

namespace SideScroll.Avalonia;

public static class AvaloniaAssets
{
	public const string AssetPath = "SideScroll.Avalonia.Assets";

	public static Assembly Assembly => Assembly.GetExecutingAssembly();

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
	}

	public static class Images
	{
		public static Image Help => GetImage(Bitmaps.Help);
		public static Image Info => GetImage(Bitmaps.Info);
	}

	public static ResourceView Get(string resourceName, string resourceType) => new(Assembly, AssetPath, "Themes", resourceName, resourceType);
	public static string GetText(string resourceName, string resourceType) => Get(resourceName, resourceType).ReadText();

	public static class Themes
	{
		public static string LightBlue => GetText("LightBlue", "json");
	}
}
