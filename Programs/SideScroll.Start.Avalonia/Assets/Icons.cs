using SideScroll.Resources;
using System.Reflection;

namespace SideScroll.Start.Avalonia.Assets;

public static class Icons
{
	public const string IconPath = "SideScroll.Start.Avalonia.Assets";

	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	public static ResourceView Logo => new(Assembly, IconPath, "Logo", "SideScroll", "ico");
}
