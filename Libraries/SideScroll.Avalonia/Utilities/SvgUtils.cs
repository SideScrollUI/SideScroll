using Avalonia.Media;
using Avalonia.Svg.Skia;
using SideScroll.Avalonia.Themes;
using SideScroll.Collections;
using SideScroll.Resources;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Avalonia.Utilities;

/// <summary>
/// Provides utility methods for loading and processing SVG images in Avalonia
/// </summary>
public static class SvgUtils
{
	private static readonly MemoryTypeCache<IImage> _imageCache = new();

	/// <summary>
	/// Loads an SVG image from a resource with optional color customization
	/// </summary>
	public static IImage GetSvgColorImage(IResourceView imageResource, Color? color = null)
	{
		color ??= (imageResource as ImageColorView)?.Color;
		color ??= SideScrollTheme.IconForeground.Color;
		string key = $"{imageResource.Path}:{color}";

		lock (_imageCache)
		{
			if (_imageCache.TryGetValue(key, out IImage? image)) return image;

			IImage colorImage = GetSvgColorImage(imageResource.Stream, color);
			_imageCache.Set(key, colorImage);
			return colorImage;
		}
	}

	/// <summary>
	/// Attempts to load an SVG image from a resource with optional color customization
	/// </summary>
	public static IImage? TryGetSvgColorImage(IResourceView imageResource, Color? color = null)
	{
		if (imageResource.ResourceType != "svg") return null;

		try
		{
			return GetSvgColorImage(imageResource, color);
		}
		catch (Exception e)
		{
			// A native library that can't be loaded is an environment this icon can't be rendered
			// in, not a resource that's wrong, so it isn't asserted on. Svg.Skia rasterizes through
			// SkiaSharp regardless of Avalonia's render backend, and that needs libfontconfig.so.1,
			// which a minimal Linux container often doesn't have. Returning null is already the
			// behavior a Release build has, where Debug.Fail compiles away
			if (!IsMissingNativeLibrary(e))
			{
				Debug.Fail(e.ToString());
			}
			return null;
		}
	}

	/// <summary>
	/// Whether an exception was caused by a native library that couldn't be loaded
	/// </summary>
	/// <remarks>
	/// Walks the whole chain rather than checking the exception itself. The first access wraps the
	/// <see cref="DllNotFoundException"/> in a <see cref="TypeInitializationException"/> for the
	/// type whose initializer called into it, and every access afterwards rethrows that same
	/// wrapper, so neither one is the missing library exception directly
	/// </remarks>
	internal static bool IsMissingNativeLibrary(Exception? e)
	{
		for (; e != null; e = e.InnerException)
		{
			if (e is DllNotFoundException) return true;
		}
		return false;
	}

	/// <summary>
	/// Loads an SVG image from a stream and replaces black colors and the currentColor with the specified color
	/// </summary>
	public static IImage GetSvgColorImage(Stream stream, Color? color = null)
	{
		stream.Position = 0;

		// The stream belongs to the caller, disposing the reader used to close it out from under
		// them. IsSvg() below reads the same way
		using var reader = new StreamReader(stream, leaveOpen: true);
		string text = reader.ReadToEnd();
		Color newColor = color ?? SideScrollTheme.IconForeground.Color;
		string newColorText = $"rgba({newColor.R},{newColor.G},{newColor.B},{newColor.A})";
		string updated = text
			.Replace("#000000", newColorText)
			.Replace("rgb(0,0,0)", newColorText)
			.Replace("currentColor", newColorText);

		return new SvgImage
		{
			Source = SvgSource.LoadFromSvg(updated),
			//Css = "path { fill:#ff0000; }", // throws Exception
		};
	}

	/// <summary>
	/// Attempts to load an SVG image from a file path
	/// </summary>
	public static bool TryGetSvgImage(Call call, string path, [NotNullWhen(true)] out IImage? image)
	{
		image = null;

		if (!HasSvgExtension(path)) return false;

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

	/// <summary>
	/// Returns whether the path names an SVG file
	/// </summary>
	/// <remarks>
	/// Ordinal, where ToLower() and EndsWith(string) both use the current culture, matching how
	/// FileTypeDetector and TabFile.ExtensionTypes compare extensions. The culture treats a zero
	/// width space as ignorable, so a file the OS reads as another extension matched here
	/// </remarks>
	internal static bool HasSvgExtension(string path)
	{
		return path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>Loads an SVG image from a resource without any color replacement, throwing if the resource is not an SVG.</summary>
	public static IImage GetSvgImage(IResourceView imageResource)
	{
		if (imageResource.ResourceType != "svg")
		{
			throw new Exception("File path must end with a .svg extension");
		}
		using var reader = new StreamReader(imageResource.Stream);

		return new SvgImage
		{
			Source = SvgSource.LoadFromSvg(reader.ReadToEnd()),
		};
	}

	/// <summary>
	/// Determines if a stream contains SVG content by checking for XML header
	/// </summary>
	public static bool IsSvg(Stream stream)
	{
		long? originalPosition = null;
		try
		{
			originalPosition = stream.Position;
			if (stream.Length < 10) return false;

			stream.Position = 0;
			using var svgStream = new StreamReader(stream, leaveOpen: true);
			string line = svgStream.ReadLine()!;
			return line.StartsWith("<?xml", StringComparison.Ordinal);
		}
		catch (Exception)
		{
			return false;
		}
		finally
		{
			if (originalPosition is { } position)
			{
				try
				{
					stream.Position = position;
				}
				catch (Exception)
				{
					// Content detection is best effort, including restoration if the stream became unavailable
				}
			}
		}
	}
}
