using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Atlas.Resources
{
	public class Icons
	{
		public static readonly string Logo = "Logo3.ico";

		public static readonly string Pin = "placeholder.png";
		public static readonly string Add = "add.png";
		public static readonly string Delete = "cancel.png"; // red too bright

		public static readonly string Back = "left-arrow-blue.png";
		public static readonly string Forward = "right-arrow-blue.png";

		public static readonly string Search = "search.png";
		public static readonly string ClearSearch = "clear_search.png";

		public static readonly string Info1 = "info_24_759eeb.png"; // 759eeb
		//public static readonly string Info2 = "info_24_c8c2f9.png"; // C8C2F9

		public static readonly string BlankDocument = "blank-document.png";
		public static readonly string Save = "save-file-option.png";

		public static readonly string Browser = "internet.png";

		public static readonly string Unlock = "unlock.png";
		public static readonly string Password = "password.png";

		public static readonly string PadNote = "padnote.png";
		public static readonly string Paste = "paste_16.png";
		public static readonly string Eraser = "eraser.png";

		public static readonly string Refresh = "refresh.png";
		public static readonly string Stats = "stats.png";

		public static readonly string List1 = "list1.png";
		public static readonly string List2 = "list2.png";
		public static readonly string DeleteList = "delete_list.png";

		public static readonly string Link = "link.png";
		public static readonly string Bookmark = "bookmark.png";
		public static readonly string Import = "import.png";

		public static readonly string Screenshot = "screenshot.png";

		public class Streams
		{
			public static Stream Logo => Get(Icons.Logo, "Logo");

			public static Stream Pin => Get(Icons.Pin);
			public static Stream Add => Get(Icons.Add);
			public static Stream Delete => Get(Icons.Delete);

			public static Stream Back => Get(Icons.Back);
			public static Stream Forward => Get(Icons.Forward);

			public static Stream ClearSearch => Get(Icons.ClearSearch);
			public static Stream Search => Get(Icons.Search);

			public static Stream Info => Get(Icons.Info1);

			public static Stream BlankDocument => Get(Icons.BlankDocument);
			public static Stream Save => Get(Icons.Save);

			public static Stream Browser => Get(Icons.Browser);

			public static Stream Unlock => Get(Icons.Unlock);
			public static Stream Password => Get(Icons.Password);

			public static Stream PadNote => Get(Icons.PadNote);
			public static Stream Paste => Get(Icons.Paste);
			public static Stream Eraser => Get(Icons.Eraser);

			public static Stream Refresh => Get(Icons.Refresh);
			public static Stream Stats => Get(Icons.Stats);

			public static Stream List1 => Get(Icons.List1);
			public static Stream List2 => Get(Icons.List2);
			public static Stream DeleteList => Get(Icons.DeleteList);

			public static Stream Link => Get(Icons.Link);
			public static Stream Bookmark => Get(Icons.Bookmark);
			public static Stream Import => Get(Icons.Import);
			public static Stream Screenshot => Get(Icons.Screenshot);

			public static Stream Get(string resourceName, string resourceType = "png")
			{
				var assembly = Assembly.GetExecutingAssembly();
				return assembly.GetManifestResourceStream("Atlas.Resources.Icons." + resourceType + "." + resourceName);
			}

			// this might slow loading?
			public static List<Stream> All { get; set; } = new List<Stream>()
			{
				Logo,
				Pin,
				Add,
				Delete,
				Back,
				Forward,
				Search,
				ClearSearch,
				Info,
				BlankDocument,
				Save,
				Browser,
				Unlock,
				Password,
				PadNote,
				Paste,
				Eraser,
				Refresh,
				Stats,
				List1,
				List2,
				DeleteList,
				Link,
				Bookmark,
				Import,
				Screenshot,
			};
		}
	}

}
