using Atlas.Core;
using System;
using System.Text;

namespace Atlas.Network
{
	public class CachedHttp : HTTP
	{
		public HttpCache HttpCache;

		public CachedHttp(Call call, HttpCache httpCache) : 
			base(call)
		{
			HttpCache = httpCache;
		}

		public override byte[] GetBytes(string uri)
		{
			byte[] bytes = HttpCache.GetBytes(uri);
			if (bytes != null)
				return bytes;

			bytes = base.GetBytes(uri);
			HttpCache.AddEntry(uri, bytes);
			return bytes;
		}

		public override string GetString(string uri, string accept = null)
		{
			byte[] bytes = GetBytes(uri);
			if (bytes != null)
				return Encoding.ASCII.GetString(bytes);
			return null;
		}
	}
}
