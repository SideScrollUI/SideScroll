using Avalonia.Media;
using SideScroll.Resources;

namespace SideScroll.Avalonia.Themes;

/// <summary>
/// An <see cref="IResourceView"/> wrapper that resolves its normal and highlight tint colors
/// dynamically from the current <see cref="SideScrollTheme"/> at the time each color is read.
/// </summary>
public class ImageColorView(ResourceView resourceView, string colorName, string highlightColorName) : IResourceView
{
	/// <summary>Gets the resource type (e.g. "svg") of the underlying image resource.</summary>
	public string ResourceType => resourceView.ResourceType;

	/// <summary>Gets the path of the underlying image resource.</summary>
	public string Path => resourceView.Path;

	/// <summary>Gets a stream containing the raw image data.</summary>
	public Stream Stream => resourceView.Stream;

	/// <summary>Gets the normal-state tint color resolved from the current theme.</summary>
	public Color? Color => SideScrollTheme.GetBrush(colorName).Color;

	/// <summary>Gets the pointer-hover tint color resolved from the current theme.</summary>
	public Color? HighlightColor => SideScrollTheme.GetBrush(highlightColorName).Color;

	/// <summary>Creates an <see cref="ImageColorView"/> using the alternate (non-primary) icon foreground brushes.</summary>
	public static ImageColorView CreateAlternate(ResourceView resourceView)
	{
		return new ImageColorView(resourceView, "IconAltForegroundBrush", "IconAltForegroundHighlightBrush");
	}
}
