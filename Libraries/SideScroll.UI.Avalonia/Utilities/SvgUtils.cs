using Avalonia.Media;
using Avalonia.Svg.Skia;
using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.UI.Avalonia.Themes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.UI.Avalonia.Utilities;

public static class SvgUtils
{
	private static readonly MemoryTypeCache<IImage> _imageCache = new();

	public static IImage GetSvgColorImage(IResourceView imageResource, Color? color = null)
	{
		color ??= (imageResource as ImageColorView)?.Color;
		color ??= SideScrollTheme.IconForeground.Color;
		string key = $"{imageResource.Path}:{color}";

		lock (_imageCache)
		{
			if (_imageCache.TryGetValue(key, out IImage? image)) return image!;

			IImage colorImage = GetSvgColorImage(imageResource.Stream, color);
			_imageCache.Set(key, colorImage);
			return colorImage;
		}
	}

	public static IImage? TryGetSvgColorImage(IResourceView imageResource, Color? color = null)
	{
		if (imageResource.ResourceType != "svg") return null;

		try
		{
			return GetSvgColorImage(imageResource, color);
		}
		catch (Exception e)
		{
			Debug.Fail(e.ToString());
			return null;
		}
	}

	public static IImage GetSvgColorImage(Stream stream, Color? color = null)
	{
		stream.Position = 0;

		using var reader = new StreamReader(stream);
		string text = reader.ReadToEnd();
		Color newColor = color ?? SideScrollTheme.IconForeground.Color;
		string newColorText = $"rgb({newColor.R},{newColor.G},{newColor.B})";
		string updated = text
			.Replace("rgb(0,0,0)", newColorText)
			.Replace("currentColor", newColorText);

		return new SvgImage
		{
			Source = SvgSource.LoadFromSvg(updated),
			//Css = "path { fill:#ff0000; }", // throws Exception
		};
	}

	public static bool TryGetSvgImage(Call call, string path, [NotNullWhen(true)] out IImage? image)
	{
		image = default;

		if (!path.ToLower().EndsWith(".svg")) return false;

		try
		{
			string text = File.ReadAllText(path);

			image = new SvgImage
			{
				Source = SvgSource.LoadFromSvg(text),
			};
			return true;
		}
		catch (Exception e)
		{
			call.Log.Add(e);
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
