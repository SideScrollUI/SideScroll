using Atlas.Core;

namespace Atlas.Tabs
{
	public class Linker
	{
		private static string atlasPrefix = @"atlas://";

		public virtual string GetLinkUri(Call call, Bookmark bookmark)
		{
			string encoded = bookmark.GetEncodedString();
			string uri = atlasPrefix + encoded;
			return uri;
		}

		public virtual Bookmark GetBookmark(Call call, string uri, bool checkVersion)
		{
			if (!uri.StartsWith(atlasPrefix))
				return null;

			string data = uri.Substring(atlasPrefix.Length);
			if (data == null)
				return null;

			Bookmark bookmark = Bookmark.Create(data);
			return bookmark;
		}
	}
}
