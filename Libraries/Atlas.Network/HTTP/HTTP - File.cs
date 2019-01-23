using System;
using Atlas.Core;

namespace Atlas.Network
{
	public class HttpFile
	{		
		public Uri Uri { get; set; }
		public int? Size { get; set; }

		public override string ToString()
		{
			return Uri.Query;
		}

		public void Download(Call call, HttpCache httpCache)
		{
			CachedHTTP cachedHttp = new CachedHTTP(call, httpCache);
			byte[] bytes = cachedHttp.GetBytes(Uri.ToString());
			Size = bytes.Length;
		}
	}
}
