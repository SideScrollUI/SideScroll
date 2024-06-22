using SideScroll;
using System.Reflection;

namespace SideScroll.Resources;

public static class Assets
{
	public class Png : NamedItemCollection<Png, ResourceView>
	{
		public static ResourceView Hourglass => Get("hourglass64");
		public static ResourceView Shutter => Get("shutter64");

		public static Assembly Assembly => Assembly.GetExecutingAssembly();

		public const string AssetPath = "SideScroll.Resources.Assets";

		public static ResourceView Get(string resourceName) => new ResourceView(Assembly, AssetPath, "png", resourceName, "png");
	}
}
