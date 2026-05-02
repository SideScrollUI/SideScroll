using SideScroll.Collections;
using System.Reflection;

namespace SideScroll.Resources;

/// <summary>
/// Provides access to SideScroll logo resources as embedded SVG files.
/// </summary>
public static class Logo
{
	/// <summary>The embedded resource path prefix for logo files.</summary>
	public const string LogoPath = "SideScroll.Resources.Logo";

	/// <summary>Gets the assembly containing the logo resources.</summary>
	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	/// <summary>
	/// Provides named access to SVG logo variants.
	/// </summary>
	public class Svg : NamedItemCollection<Svg, ResourceView>
	{
		/// <summary>Gets the translucent SideScroll logo SVG resource.</summary>
		public static ResourceView SideScrollTranslucent => Get("SideScroll-Translucent");

		/// <summary>Gets a logo SVG resource by name.</summary>
		public static ResourceView Get(string resourceName) => new(Assembly, LogoPath, "svg", resourceName, "svg");
	}
}
