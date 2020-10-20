using Atlas.Core;

namespace Atlas.Tabs
{
	public class Linker
	{
		private const string AtlasPrefix = @"atlas://";

		public bool PublicOnly { get; set; }
		public long MaxLength { get; set; } = 100000;

		public virtual string GetLinkUri(Call call, Bookmark bookmark)
		{
			string base64 = bookmark.ToBase64String(call, PublicOnly);
			if (base64.Length > MaxLength)
				return "Bookmark size " + base64.Length + " > " + MaxLength;

			return AtlasPrefix + base64;
		}

		public virtual Bookmark GetBookmark(Call call, string uri, bool checkVersion)
		{
			if (!uri.StartsWith(AtlasPrefix))
				return null;

			string base64 = uri.Substring(AtlasPrefix.Length);
			if (base64 == null)
				return null;

			if (uri.Length > MaxLength)
				return null;

			Bookmark bookmark = Bookmark.Create(call, base64, PublicOnly);
			return bookmark;
		}
	}
}
