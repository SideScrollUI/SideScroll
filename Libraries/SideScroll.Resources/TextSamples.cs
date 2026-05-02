using SideScroll.Collections;
using System.Reflection;

namespace SideScroll.Resources;

/// <summary>
/// Provides access to embedded sample text files (plain text, JSON, and XML) for use in demos and tests.
/// </summary>
public class TextSamples : NamedItemCollection<TextSamples, ResourceView>
{
	/// <summary>The embedded resource path prefix for sample text files.</summary>
	public const string SamplePath = "SideScroll.Resources.Samples";

	/// <summary>Gets the assembly containing the sample text resources.</summary>
	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	/// <summary>Gets the plain text sample content.</summary>
	public static string Plain => GetText("SolarSystem", "txt");

	/// <summary>Gets the JSON sample content.</summary>
	public static string Json => GetText("SolarSystem", "json");

	/// <summary>Gets the XML sample content.</summary>
	public static string Xml => GetText("SolarSystem", "xml");

	/// <summary>Gets a resource view for a sample file by name and extension.</summary>
	public static ResourceView Get(string resourceName, string resourceType) => new(Assembly, SamplePath, "Text", resourceName, resourceType);

	/// <summary>Reads and returns the text content of a sample file by name and extension.</summary>
	public static string GetText(string resourceName, string resourceType) => Get(resourceName, resourceType).ReadText();
}
