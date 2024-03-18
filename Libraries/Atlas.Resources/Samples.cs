using Atlas.Core;
using System.Reflection;

namespace Atlas.Resources;

public static class Samples
{
	public const string SamplePath = "Atlas.Resources.Samples";

	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	public class Text : NamedItemCollection<Text, ResourceView>
	{
		public static string Plain => GetText("SolarSystem", "txt");
		public static string Json => GetText("SolarSystem", "json");
		public static string Xml => GetText("SolarSystem", "xml");

		public static ResourceView Get(string resourceName, string resourceType) => new(Assembly, SamplePath, "Text", resourceName, resourceType);
		public static string GetText(string resourceName, string resourceType) => Get(resourceName, resourceType).ReadText();
	}
}
