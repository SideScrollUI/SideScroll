using System.Drawing;
using System.Reflection;

namespace SideScroll.Resources;

public interface IResourceView
{
	string ResourceType { get; }
	string Path { get; }

	Stream Stream { get; }
}

public record ResourceView(Assembly Assembly, string BasePath, string GroupPath, string ResourceName, string ResourceType) : IResourceView
{
	public string Path => $"{BasePath}.{GroupPath}.{ResourceName}.{ResourceType}";

	public Stream Stream => Assembly.GetManifestResourceStream(Path)!;

	public string ReadText() => new StreamReader(Stream).ReadToEnd();
}

public class ImageResourceView(ResourceView resourceView, Color? color = null, Color? highlightColor = null) : IResourceView
{
	public string ResourceType => resourceView.ResourceType;
	public string Path => resourceView.Path;

	public Stream Stream => resourceView.Stream;

	public Color? Color => color;
	public Color? HighlightColor => highlightColor;
}
