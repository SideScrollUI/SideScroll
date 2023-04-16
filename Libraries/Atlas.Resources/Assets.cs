using Atlas.Core;

namespace Atlas.Resources;

public static class Assets
{
	public class Png : NamedItemCollection<Png, ResourceView>
	{
		public static ResourceView Hourglass => Get("hourglass64");
		public static ResourceView Shutter => Get("shutter64");

		public const string AssetPath = "Atlas.Resources.Assets";

		public static ResourceView Get(string resourceName) => new ResourceView(AssetPath, "png", resourceName, "png");
	}
}
