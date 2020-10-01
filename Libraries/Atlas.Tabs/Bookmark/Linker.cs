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
			string base64 = bookmark.ToBase64String(call);
			if (base64.Length > MaxLength)
				return "Serialization size " + base64.Length + " > " + MaxLength;
			string uri = AtlasPrefix + base64;
			return uri;
		}

		public virtual Bookmark GetBookmark(Call call, string uri, bool checkVersion)
		{
			if (!uri.StartsWith(AtlasPrefix))
				return null;

			string data = uri.Substring(AtlasPrefix.Length);
			if (data == null)
				return null;

			if (uri.Length > MaxLength)
				return null;

			Bookmark bookmark = Bookmark.Create(call, data, PublicOnly);
			return bookmark;
		}
	}
}
