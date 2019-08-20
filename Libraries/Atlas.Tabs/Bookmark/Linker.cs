using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Tabs
{
	public class Linker
	{
		public virtual string GetLinkUri(Bookmark bookmark)
		{
			string encoded = bookmark.GetEncodedString();
			string uri = "atlas:" + encoded;
			return uri;
		}

		public virtual string GetLinkData(string uri)
		{
			if (!uri.StartsWith("atlas:"))
				return null;

			string data = uri.Substring(6);
			return data;
		}
	}
}
