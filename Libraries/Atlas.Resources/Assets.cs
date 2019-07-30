using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Atlas.Resources
{
	public class Assets
	{
		public static readonly string Logo = "Logo.ico";

		public static readonly string Pin = "placeholder.png";
		public static readonly string Add = "add.png";
		public static readonly string Delete = "cancel.png"; // red too bright

		public static readonly string Back = "left-arrow-blue.png";
		public static readonly string Forward = "right-arrow-blue.png";

		public static readonly string Search = "search.png";

		public static readonly string Info1 = "info_24_759eeb.png"; // 759eeb
		//public static readonly string Info2 = "info_24_c8c2f9.png"; // C8C2F9

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

		public class Streams
		{
			public static Stream Logo => Get(Assets.Logo);

			public static Stream Pin => Get(Assets.Pin);
			public static Stream Add => Get(Assets.Add);
			public static Stream Delete => Get(Assets.Delete);

			public static Stream Back => Get(Assets.Back);
			public static Stream Forward => Get(Assets.Forward);

			public static Stream Search => Get(Assets.Search);

			public static Stream Info => Get(Assets.Info1);

			public static Stream Save => Get(Assets.Save);

			public static Stream Browser => Get(Assets.Browser);

			public static Stream Unlock => Get(Assets.Unlock);
			public static Stream Password => Get(Assets.Password);

			public static Stream PadNote => Get(Assets.PadNote);
			public static Stream Paste => Get(Assets.Paste);
			public static Stream Eraser => Get(Assets.Eraser);

			public static Stream Refresh => Get(Assets.Refresh);
			public static Stream Stats => Get(Assets.Stats);

			public static Stream List1 => Get(Assets.List1);
			public static Stream List2 => Get(Assets.List2);
			public static Stream DeleteList => Get(Assets.DeleteList);

			public static Stream Link => Get(Assets.Link);
			public static Stream Bookmark => Get(Assets.Bookmark);
			public static Stream Import => Get(Assets.Import);

			public static Stream Get(string resourceName)
			{
				var assembly = Assembly.GetExecutingAssembly();
				return assembly.GetManifestResourceStream("Atlas.Resources.Assets." + resourceName);
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
				Info,
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
			};
		}
	}

}
