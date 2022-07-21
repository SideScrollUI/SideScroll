using Atlas.Core;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.Network;

public class HttpCachedCall : HttpCall
{
	public HttpCache HttpCache;

	public HttpCachedCall(Call call, HttpCache httpCache) :
		base(call)
	{
		HttpCache = httpCache;
	}

	public override async Task<byte[]> GetBytesAsync(string uri)
	{
		byte[]? bytes = HttpCache.GetBytes(uri);
		if (bytes != null)
			return bytes;

		bytes = await base.GetBytesAsync(uri);
		HttpCache.AddEntry(uri, bytes);
		return bytes;
	}

	public override async Task<string?> GetStringAsync(string uri, string? accept = null)
	{
		byte[] bytes = await GetBytesAsync(uri);
		if (bytes != null)
			return Encoding.ASCII.GetString(bytes);

		return null;
	}
}
