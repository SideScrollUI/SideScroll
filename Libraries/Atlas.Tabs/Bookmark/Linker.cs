using System;
using System.Collections.Generic;

namespace Atlas.Tabs
{
	public class Linker
	{
		private static string atlasPrefix = @"atlas://";

		public virtual string GetLinkUri(Bookmark bookmark)
		{
			string encoded = bookmark.GetEncodedString();
			string uri = atlasPrefix + encoded;
			return uri;
		}

		public virtual Bookmark GetBookmark(string uri)
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
