using System.Reflection;

namespace Atlas.Resources;

interface IResourceView
{
	string Path { get; }

	Stream Stream { get; }
}

public record ResourceView(string BasePath, string? GroupPath, string ResourceName, string ResourceType) : IResourceView
{
	public string Path => $"{BasePath}.{GroupPath}.{ResourceName}.{ResourceType}";

	public Stream Stream => Assembly.GetExecutingAssembly().GetManifestResourceStream(Path)!;
}
