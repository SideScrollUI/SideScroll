using SideScroll.Resources;
using System.Reflection;

namespace SideScroll.Avalonia.Samples.Assets;

public static class Icons
{
	public const string IconPath = "SideScroll.Avalonia.Samples.Assets";

	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	public static ResourceView SideScroll => new(Assembly, IconPath, "Logo", "SideScroll", "ico");
}
