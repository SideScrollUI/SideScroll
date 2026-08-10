using System.Drawing;
using System.Reflection;

namespace SideScroll.Resources;

/// <summary>
/// Represents a view of an embedded resource
/// </summary>
public interface IResourceView
{
	/// <summary>
	/// The resource file type/extension
	/// </summary>
	string ResourceType { get; }

	/// <summary>
	/// The full resource path
	/// </summary>
	string Path { get; }

	/// <summary>
	/// Stream for reading the resource content
	/// </summary>
	Stream Stream { get; }
}

/// <summary>
/// View of an embedded resource from an assembly
/// </summary>
public record ResourceView(Assembly Assembly, string BasePath, string GroupPath, string ResourceName, string ResourceType) : IResourceView
{
	/// <summary>
	/// The full resource path combining base path, group path, resource name, and resource type
	/// </summary>
	public string Path => $"{BasePath}.{GroupPath}.{ResourceName}.{ResourceType}";

	/// <summary>
	/// Stream for reading the embedded resource content from the assembly
	/// </summary>
	public Stream Stream => Assembly.GetManifestResourceStream(Path)
		?? throw new FileNotFoundException($"Embedded resource '{Path}' was not found in assembly '{Assembly.FullName}'.", Path);

	/// <summary>
	/// Reads the resource content as text
	/// </summary>
	/// <remarks>
	/// <see cref="Stream"/> opens a new manifest resource stream on every access, so the reader
	/// owns this one. Neither used to be disposed
	/// </remarks>
	public string ReadText()
	{
		using var reader = new StreamReader(Stream);
		return reader.ReadToEnd();
	}
}

/// <summary>
/// View of an image resource with optional color customization
/// </summary>
public class ImageResourceView(ResourceView resourceView, Color? color = null, Color? highlightColor = null) : IResourceView
{
	/// <summary>
	/// The resource file type/extension
	/// </summary>
	public string ResourceType => resourceView.ResourceType;

	/// <summary>
	/// The full resource path
	/// </summary>
	public string Path => resourceView.Path;

	/// <summary>
	/// Stream for reading the resource content
	/// </summary>
	public Stream Stream => resourceView.Stream;

	/// <summary>
	/// Optional color to apply to the image
	/// </summary>
	public Color? Color => color;

	/// <summary>
	/// Optional highlight color to apply to the image
	/// </summary>
	public Color? HighlightColor => highlightColor;
}
