using Avalonia.Media.Imaging;
using SideScroll.Resources;
using System.Reflection;

namespace SideScroll.Avalonia;

/// <summary>
/// Provides access to embedded Avalonia UI assets such as theme files and images.
/// </summary>
public static class AvaloniaAssets
{
	/// <summary>The embedded resource path prefix for Avalonia asset files.</summary>
	public const string AssetPath = "SideScroll.Avalonia.Assets";

	/// <summary>Gets the assembly containing the Avalonia assets.</summary>
	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	/// <summary>Loads an embedded bitmap resource by its fully qualified resource name.</summary>
	public static Bitmap GetBitmap(string name)
	{
		Assembly assembly = Assembly.GetExecutingAssembly();

		using Stream stream = assembly.GetManifestResourceStream(name)!;
		return new Bitmap(stream);
	}

	/// <summary>Gets a resource view for a theme asset with the given name and type.</summary>
	public static ResourceView Get(string resourceName, string resourceType) => new(Assembly, AssetPath, "Themes", resourceName, resourceType);

	/// <summary>Reads and returns the text content of a theme asset.</summary>
	public static string GetText(string resourceName, string resourceType) => Get(resourceName, resourceType).ReadText();

	/// <summary>
	/// Provides named access to bundled theme JSON files.
	/// </summary>
	public static class Themes
	{
		/// <summary>Gets the Hybrid theme JSON content.</summary>
		public static string Hybrid => GetText("Hybrid", "json");
	}
}
