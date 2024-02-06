using Atlas.Resources;
using Atlas.UI.Avalonia.Themes;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Atlas.UI.Avalonia.Utilities;

public static class SvgUtils
{
	private static Dictionary<string, IImage> _images = new();

	public static IImage? GetSvgImage(IResourceView imageResource, Color? color = null)
	{
		if (imageResource.ResourceType != "svg") return null;

		try
		{
			lock (_images)
			{
				color ??= AtlasTheme.IconForeground.Color;
				string key = $"{imageResource.Path}:{color}";
				if (_images.TryGetValue(key, out IImage? image)) return image;

				IImage queueImage = GetSvgImage(imageResource.Stream, color);
				_images[key] = queueImage;
				return queueImage;
			}
		}
		catch (Exception e)
		{
			Debug.Fail(e.ToString());
			return null;
		}
	}

	public static IImage GetSvgImage(Stream stream, Color? color = null)
	{
		stream.Position = 0;

		using var reader = new StreamReader(stream);
		string text = reader.ReadToEnd();
		Color newColor = color ?? AtlasTheme.IconForeground.Color;
		string updated = text.Replace("rgb(0,0,0)", $"rgb({newColor.R},{newColor.G},{newColor.B})");

		SvgSource svgSource = new();
		svgSource.FromSvg(updated);

		return new SvgImage()
		{
			Source = svgSource,
		};
	}

	public static bool TryGetSvgImage(string path, [NotNullWhen(true)] out IImage? image)
	{
		image = default;

		if (!path.ToLower().EndsWith(".svg")) return false;

		try
		{
			string text = File.ReadAllText(path);

			SvgSource svgSource = new();
			svgSource.FromSvg(text);

			image = new SvgImage()
			{
				Source = svgSource,
			};
			return true;
		}
		catch (Exception e)
		{
			Debug.Fail(e.ToString());
			return false;
		}
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
