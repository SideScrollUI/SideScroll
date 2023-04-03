using Atlas.Core;
using System.Reflection;

namespace Atlas.Resources;

public static class Icons
{
	public const string Logo = "Logo3.ico";
	public class Png : NamedItemCollection<Png, string>
	{
		public const string Search16 = "search_right_light_16.png";
		public const string ClearSearch = "clear_search.png";

		public const string Info1 = "info_24_759eeb.png";

		public const string Info20 = "info_20_759eeb.png";

		public const string Unlock = "unlock.png";
		public const string Password = "password.png";

		public const string Paste = "paste_16.png";

		public const string Bookmark = "bookmark.png";
	}

	public class Svg : NamedItemCollection<Svg, string>
	{
		public const string Pin = "Placeholder.svg";

		public const string Add = "Add.svg";
		public const string Delete = "Cancel.svg";

		public const string LeftArrow = "LeftArrow.svg";
		public const string RightArrow = "RightArrow.svg";
		public const string UpArrow = "UpArrow.svg";
		public const string DownArrow = "DownArrow.svg";

		public const string LeftArrowCircle = "LeftArrowCircle.svg";
		public const string RightArrowCircle = "RightArrowCircle.svg";

		public const string Search = "Search.svg";

		public const string BlankDocument = "BlankDocument.svg";

		public const string Save = "Save.svg";
		public const string OpenFolder = "OpenFolder.svg";

		public const string Star = "Star.svg";
		public const string StarFilled = "StarFilled.svg";

		public const string Browser = "Internet.svg";

		public const string Enter = "Enter.svg";

		public const string Copy = "Copy.svg";
		public const string PadNote = "PadNote.svg";
		public const string Eraser = "Eraser.svg";

		public const string Refresh = "Refresh.svg";
		public const string Stats = "Stats.svg";

		public const string List1 = "List1.svg";
		public const string List2 = "List2.svg";
		public const string DeleteList = "DeleteList.svg";

		public const string Link = "Link.svg";
		public const string Import = "Import.svg";

		public const string Screenshot = "Screenshot.svg";
	}

	public class Streams : NamedItemCollection<Streams, Stream>
	{
		public static Stream Logo => Get(Icons.Logo, "Logo");

		public static Stream Pin => GetSvg(Svg.Pin);
		public static Stream Add => GetSvg(Svg.Add);
		public static Stream Delete => GetSvg(Svg.Delete);

		public static Stream Back => GetSvg(Svg.LeftArrowCircle);
		public static Stream Forward => GetSvg(Svg.RightArrowCircle);

		public static Stream LeftArrow => GetSvg(Svg.LeftArrow);
		public static Stream RightArrow => GetSvg(Svg.RightArrow);
		public static Stream UpArrow => GetSvg(Svg.UpArrow);
		public static Stream DownArrow => GetSvg(Svg.DownArrow);

		public static Stream ClearSearch => Get(Png.ClearSearch);
		public static Stream Search => GetSvg(Svg.Search);
		public static Stream Search16 => Get(Png.Search16);

		public static Stream Info => Get(Png.Info1);
		public static Stream Info20 => Get(Png.Info20);

		public static Stream BlankDocument => GetSvg(Svg.BlankDocument);
		public static Stream Save => GetSvg(Svg.Save);
		public static Stream OpenFolder => GetSvg(Svg.OpenFolder);

		public static Stream Star => GetSvg(Svg.Star);
		public static Stream StarFilled => GetSvg(Svg.StarFilled);

		public static Stream Browser => GetSvg(Svg.Browser);

		public static Stream Unlock => Get(Png.Unlock);
		public static Stream Password => Get(Png.Password);
		public static Stream Enter => GetSvg(Svg.Enter);

		public static Stream Copy => GetSvg(Svg.Copy);
		public static Stream PadNote => GetSvg(Svg.PadNote);
		public static Stream Paste => Get(Png.Paste);
		public static Stream Eraser => GetSvg(Svg.Eraser);

		public static Stream Refresh => GetSvg(Svg.Refresh);
		public static Stream Stats => GetSvg(Svg.Stats);

		public static Stream List1 => GetSvg(Svg.List1);
		public static Stream List2 => GetSvg(Svg.List2);
		public static Stream DeleteList => GetSvg(Svg.DeleteList);

		public static Stream Link => GetSvg(Svg.Link);
		public static Stream Bookmark => Get(Png.Bookmark);
		public static Stream Import => GetSvg(Svg.Import);
		public static Stream Screenshot => GetSvg(Svg.Screenshot);

		public static Stream Get(string resourceName, string resourceType = "png")
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			return assembly.GetManifestResourceStream("Atlas.Resources.Icons." + resourceType + "." + resourceName)!;
		}

		public static Stream GetSvg(string resourceName)
		{
			return Get(resourceName, "svg");
		}
	}
}
