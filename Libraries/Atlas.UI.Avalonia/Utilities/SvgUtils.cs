using Atlas.Resources;
using Atlas.UI.Avalonia.Themes;
using Avalonia.Media;
using Avalonia.Svg.Skia;

namespace Atlas.UI.Avalonia.Utilities;

public static class SvgUtils
{
	private static Dictionary<string, IImage> _images = new();

	public static IImage GetSvgImage(string resourceName)
	{
		lock (_images)
		{
			if (_images.TryGetValue(resourceName, out IImage? image)) return image;

			Stream bitmapStream = Icons.Streams.GetSvg(resourceName);
			IImage queueImage = SvgUtils.GetSvgImage(bitmapStream);
			_images[resourceName] = queueImage;
			return queueImage;
		}
	}

	public static IImage GetSvgImage(Stream bitmapStream)
	{
		IImage sourceImage;
		bitmapStream.Position = 0;

		using var reader = new StreamReader(bitmapStream);
		string text = reader.ReadToEnd();
		Color color = Theme.IconForeground.Color;
		string updated = text.Replace("rgb(0,0,0)", $"rgb({color.R},{color.G},{color.B})");

		SvgSource svgSource = new();
		svgSource.FromSvg(updated);
		sourceImage = new SvgImage()
		{
			Source = svgSource,
		};

		return sourceImage;
	}
}
