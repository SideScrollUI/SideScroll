using SideScroll.Collections;
using System.Reflection;

namespace SideScroll.Resources;

public static class Icons
{
	public const string IconPath = "SideScroll.Resources.Icons";

	public static Assembly Assembly => Assembly.GetExecutingAssembly();

	public class Svg : NamedItemCollection<Svg, ResourceView>
	{
		public static ResourceView Refresh { get; } = Get("Refresh");

		public static ResourceView Add { get; } = Get("Add");
		public static ResourceView Delete { get; } = Get("Cancel");

		public static ResourceView Back { get; } = Get("LeftArrowCircle");
		public static ResourceView Forward { get; } = Get("RightArrowCircle");

		public static ResourceView Play { get; } = Get("PlayCircle");
		public static ResourceView Stop { get; } = Get("StopCircle");

		public static ResourceView LeftArrow { get; } = Get("LeftArrow");
		public static ResourceView RightArrow { get; } = Get("RightArrow");
		public static ResourceView UpArrow { get; } = Get("UpArrow");
		public static ResourceView DownArrow { get; } = Get("DownArrow");

		public static ResourceView Reset { get; } = Get("Reset");
		public static ResourceView Undo { get; } = Get("Undo");
		public static ResourceView Redo { get; } = Get("Redo");

		public static ResourceView Search { get; } = Get("Search");
		public static ResourceView SearchRight { get; } = Get("SearchRight");
		public static ResourceView ClearSearch { get; } = Get("ClearSearch");

		public static ResourceView BlankDocument { get; } = Get("BlankDocument");
		public static ResourceView Save { get; } = Get("Save");
		public static ResourceView OpenFolder { get; } = Get("OpenFolder");

		public static ResourceView Star { get; } = Get("Star");
		public static ResourceView StarFilled { get; } = Get("StarFilled");

		public static ResourceView Browser { get; } = Get("Internet");

		public static ResourceView Enter { get; } = Get("Enter");

		public static ResourceView Copy { get; } = Get("Copy");
		public static ResourceView PadNote { get; } = Get("PadNote");
		public static ResourceView Eraser { get; } = Get("Eraser");

		public static ResourceView Duplicate { get; } = Get("Duplicate");

		public static ResourceView Pin { get; } = Get("Placeholder");
		public static ResourceView Bookmark { get; } = Get("Bookmark");

		public static ResourceView Info { get; } = Get("Info");

		public static ResourceView Stats { get; } = Get("Stats");

		public static ResourceView List1 { get; } = Get("List1");
		public static ResourceView List2 { get; } = Get("List2");
		public static ResourceView DeleteList { get; } = Get("DeleteList");

		public static ResourceView Link { get; } = Get("Link");
		public static ResourceView Import { get; } = Get("Import");
		public static ResourceView Download { get; } = Get("Download");
		public static ResourceView Screenshot { get; } = Get("Screenshot");

		public static ResourceView PanelLeftExpand { get; } = Get("PanelLeftExpand");
		public static ResourceView PanelLeftContract { get; } = Get("PanelLeftContract");

		public static ResourceView Minimize { get; } = Get("Minimize");
		public static ResourceView Maximize { get; } = Get("Maximize");
		public static ResourceView Restore { get; } = Get("Restore");
		public static ResourceView Close { get; } = Get("Close");

		public static ResourceView Get(string resourceName) => new(Assembly, IconPath, "svg", resourceName, "svg");
	}
}
