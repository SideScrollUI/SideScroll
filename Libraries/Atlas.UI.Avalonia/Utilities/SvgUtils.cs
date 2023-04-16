using Atlas.Resources;
using Atlas.UI.Avalonia.Themes;
using Avalonia.Media;
using Avalonia.Svg.Skia;

namespace Atlas.UI.Avalonia.Utilities;

public static class SvgUtils
{
	private static Dictionary<string, IImage> _images = new();

	public static IImage GetSvgImage(ResourceView imageResource, Color? color = null)
	{
		lock (_images)
		{
			string key = $"{imageResource.Path}:{color}";
			if (_images.TryGetValue(key, out IImage? image)) return image;

			IImage queueImage = SvgUtils.GetSvgImage(imageResource.Stream, color);
			_images[key] = queueImage;
			return queueImage;
		}
	}

	public static IImage GetSvgImage(Stream bitmapStream, Color? color = null)
	{
		IImage sourceImage;
		bitmapStream.Position = 0;

		using var reader = new StreamReader(bitmapStream);
		string text = reader.ReadToEnd();
		Color newColor = color ?? Theme.IconForeground.Color;
		string updated = text.Replace("rgb(0,0,0)", $"rgb({newColor.R},{newColor.G},{newColor.B})");

		SvgSource svgSource = new();
		svgSource.FromSvg(updated);
		sourceImage = new SvgImage()
		{
			Source = svgSource,
		};

		return sourceImage;
	}

	public static bool IsSvg(Stream stream)
	{
		if (stream.Length < 10) return false;

		try
		{
			stream.Position = 0;
			using var svgStream = new StreamReader(stream, leaveOpen: true);
			string line = svgStream.ReadLine()!;
			return line.StartsWith("<?xml");
		}
		catch (Exception)
		{
			return false;
		}
	}
}
