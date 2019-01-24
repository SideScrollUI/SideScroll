using System;
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

			public static Stream Get(string resourceName)
			{
				var assembly = Assembly.GetExecutingAssembly();
				return assembly.GetManifestResourceStream("Atlas.Resources.Assets." + resourceName);
			}
		}
	}

}
