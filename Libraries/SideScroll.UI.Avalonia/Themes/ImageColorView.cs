using SideScroll.Resources;
using Avalonia.Media;

namespace SideScroll.UI.Avalonia.Themes;

public class ImageColorView(ResourceView resourceView, string colorName, string highlightColorName) : IResourceView
{
	public string ResourceType => resourceView.ResourceType;
	public string Path => resourceView.Path;

	public Stream Stream => resourceView.Stream;

	public Color? Color => SideScrollTheme.GetBrush(colorName).Color;
	public Color? HighlightColor => SideScrollTheme.GetBrush(highlightColorName).Color;

	public static ImageColorView CreateAlternate(ResourceView resourceView)
	{
		return new ImageColorView(resourceView, "IconAltForegroundBrush", "IconAltForegroundHighlightBrush");
	}
}
