using Atlas.Core;
using System;
using System.Text;

namespace Atlas.Network
{
	public class CachedHTTP : HTTP
	{
		private HttpCache httpCache;

		public CachedHTTP(Call call, HttpCache httpCache) : 
			base(call)
		{
			this.httpCache = httpCache;
		}

		public override byte[] GetBytes(string uri)
		{
			byte[] bytes = httpCache.GetBytes(uri);
			if (bytes != null)
				return bytes;

			bytes = base.GetBytes(uri);
			httpCache.AddEntry(uri, bytes);
			return bytes;
		}

		public override string GetString(string uri)
		{
			byte[] bytes = GetBytes(uri);
			if (bytes != null)
				return Encoding.ASCII.GetString(bytes);
			return null;
		}
	}
}
