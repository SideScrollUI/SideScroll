using SideScroll.Collections;
using System.Reflection;

namespace SideScroll.Resources;

public static class Logo
{
	public const string LogoPath = "SideScroll.Resources.Logo";

	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	public class Svg : NamedItemCollection<Svg, ResourceView>
	{
		public static ResourceView SideScroll => Get("SideScroll");

		public static ResourceView Get(string resourceName) => new(Assembly, LogoPath, "svg", resourceName, "svg");
	}
}
