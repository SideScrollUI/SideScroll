using Atlas.Core;
using System.Reflection;

namespace Atlas.Resources;

public static class Icons
{
	public const string IconPath = "Atlas.Resources.Icons";

	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	public static ResourceView Logo => new(Assembly, IconPath, "Logo", "Logo3", "ico");

	public class Svg : NamedItemCollection<Svg, ResourceView>
	{
		public static ResourceView Refresh => Get("Refresh");

		public static ResourceView Add => Get("Add");
		public static ResourceView Delete => Get("Cancel");

		public static ResourceView Back => Get("LeftArrowCircle");
		public static ResourceView Forward => Get("RightArrowCircle");

		public static ResourceView LeftArrow => Get("LeftArrow");
		public static ResourceView RightArrow => Get("RightArrow");
		public static ResourceView UpArrow => Get("UpArrow");
		public static ResourceView DownArrow => Get("DownArrow");

		public static ResourceView Undo => Get("Undo");
		public static ResourceView Redo => Get("Redo");

		public static ResourceView Search => Get("Search");

		public static ResourceView BlankDocument => Get("BlankDocument");
		public static ResourceView Save => Get("Save");
		public static ResourceView OpenFolder => Get("OpenFolder");

		public static ResourceView Star => Get("Star");
		public static ResourceView StarFilled => Get("StarFilled");

		public static ResourceView Browser => Get("Internet");

		public static ResourceView Enter => Get("Enter");

		public static ResourceView Copy => Get("Copy");
		public static ResourceView PadNote => Get("PadNote");
		public static ResourceView Eraser => Get("Eraser");

		public static ResourceView Pin => Get("Placeholder");

		public static ResourceView Stats => Get("Stats");

		public static ResourceView List1 => Get("List1");
		public static ResourceView List2 => Get("List2");
		public static ResourceView DeleteList => Get("DeleteList");

		public static ResourceView Link => Get("Link");
		public static ResourceView Import => Get("Import");
		public static ResourceView Screenshot => Get("Screenshot");

		public static ResourceView Get(string resourceName) => new(Assembly, IconPath, "svg", resourceName, "svg");
	}

	public class Png : NamedItemCollection<Png, ResourceView>
	{
		public static ResourceView ClearSearch => Get("clear_search");
		public static ResourceView Search16 => Get("search_right_light_16");

		public static ResourceView Info => Get("info_24_759eeb");
		public static ResourceView Info20 => Get("info_20_759eeb");

		public static ResourceView Unlock => Get("unlock");
		public static ResourceView Password => Get("password");

		public static ResourceView Paste => Get("paste_16");

		public static ResourceView Bookmark => Get("bookmark");

		public static ResourceView Get(string resourceName) => new(Assembly, IconPath, "png", resourceName, "png");
	}
}
