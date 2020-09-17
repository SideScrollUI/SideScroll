using Atlas.Core;
using System;

namespace Atlas.Network
{
	public class HttpFile
	{		
		public Uri Uri { get; set; }
		public int? Size { get; set; }

		public override string ToString() => Uri.Query;

		public void Download(Call call, HttpCache httpCache)
		{
			var cachedHttp = new CachedHttp(call, httpCache);
			byte[] bytes = cachedHttp.GetBytes(Uri.ToString());
			Size = bytes.Length;
		}
	}
}
