using SideScroll.Collections;
using System.Reflection;

namespace SideScroll.Resources;

public static class Icons
{
	public const string IconPath = "SideScroll.Resources.Icons";

	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	public class Svg : NamedItemCollection<Svg, ResourceView>
	{
		public static ResourceView Refresh => Get("Refresh");

		public static ResourceView Add => Get("Add");
		public static ResourceView Delete => Get("Cancel");

		public static ResourceView Back => Get("LeftArrowCircle");
		public static ResourceView Forward => Get("RightArrowCircle");

		public static ResourceView Play => Get("PlayCircle");
		public static ResourceView Stop => Get("StopCircle");

		public static ResourceView LeftArrow => Get("LeftArrow");
		public static ResourceView RightArrow => Get("RightArrow");
		public static ResourceView UpArrow => Get("UpArrow");
		public static ResourceView DownArrow => Get("DownArrow");

		public static ResourceView Reset => Get("Reset");
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

		public static ResourceView Duplicate => Get("Duplicate");

		public static ResourceView Pin => Get("Placeholder");
		public static ResourceView Bookmark => Get("Bookmark");

		public static ResourceView Info => Get("Info");

		public static ResourceView Stats => Get("Stats");

		public static ResourceView List1 => Get("List1");
		public static ResourceView List2 => Get("List2");
		public static ResourceView DeleteList => Get("DeleteList");

		public static ResourceView Link => Get("Link");
		public static ResourceView Import => Get("Import");
		public static ResourceView Download => Get("Download");
		public static ResourceView Screenshot => Get("Screenshot");

		public static ResourceView PanelLeftExpand => Get("PanelLeftExpand");
		public static ResourceView PanelLeftContract => Get("PanelLeftContract");

		public static ResourceView Get(string resourceName) => new(Assembly, IconPath, "svg", resourceName, "svg");
	}

	// todo: Deprecate
	public class Png : NamedItemCollection<Png, ResourceView>
	{
		public static ResourceView ClearSearch => Get("clear_search");
		public static ResourceView Search16 => Get("search_right_light_16");

		public static ResourceView Get(string resourceName) => new(Assembly, IconPath, "png", resourceName, "png");
	}
}
