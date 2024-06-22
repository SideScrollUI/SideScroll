using SideScroll.Collections;
using System.Reflection;

namespace SideScroll.Resources;

public class TextSamples : NamedItemCollection<TextSamples, ResourceView>
{
	public const string SamplePath = "SideScroll.Resources.Samples";

	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	public static string Plain => GetText("SolarSystem", "txt");
	public static string Json => GetText("SolarSystem", "json");
	public static string Xml => GetText("SolarSystem", "xml");

	public static ResourceView Get(string resourceName, string resourceType) => new(Assembly, SamplePath, "Text", resourceName, resourceType);
	public static string GetText(string resourceName, string resourceType) => Get(resourceName, resourceType).ReadText();
}
