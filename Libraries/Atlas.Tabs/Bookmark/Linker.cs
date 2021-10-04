using Atlas.Core;

namespace Atlas.Tabs
{
	public class Linker
	{
		private const string AtlasPrefix = @"atlas://";

		public bool PublicOnly { get; set; }
		public long MaxLength { get; set; } = 65500; // Uri.EscapeDataString limit

		public virtual string GetLinkUri(Call call, Bookmark bookmark)
		{
#if DEBUG
			call = call.DebugLogAll();
#endif
			string base64 = bookmark.ToBase64String(call, PublicOnly);
			if (base64.Length > MaxLength)
				return "Bookmark size " + base64.Length + " > " + MaxLength;

			return AtlasPrefix + base64;
		}

		public virtual Bookmark GetBookmark(Call call, string uri, bool checkVersion)
		{
			if (uri == null)
				return null;
#if DEBUG
			call = call.DebugLogAll();
#endif

			if (!uri.StartsWith(AtlasPrefix))
			{
				call.Log.AddError("Invalid prefix");
				return null;
			}

			string base64 = uri.Substring(AtlasPrefix.Length);

			if (uri.Length > MaxLength)
			{
				call.Log.AddError("Bookmark too large", 
					new Tag("Length", uri.Length), 
					new Tag("MaxLength", MaxLength));
				return null;
			}

			Bookmark bookmark = Bookmark.Create(call, base64, PublicOnly);
			return bookmark;
		}
	}
}
