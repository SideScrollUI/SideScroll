using Atlas.Resources;
using Avalonia.Media;

namespace Atlas.UI.Avalonia.Themes;

public class ImageColorView(ResourceView resourceView, string colorName, string highlightColorName) : IResourceView
{
	public string ResourceType => resourceView.ResourceType;
	public string Path => resourceView.Path;

	public Stream Stream => resourceView.Stream;

	public Color? Color => AtlasTheme.GetBrush(colorName).Color;
	public Color? HighlightColor => AtlasTheme.GetBrush(highlightColorName).Color;

	public static ImageColorView CreateAlternate(ResourceView resourceView)
	{
		return new ImageColorView(resourceView, "IconAltForegroundBrush", "IconAltForegroundHighlightBrush");
	}
}
