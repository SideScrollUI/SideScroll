using System.Text;

namespace SideScroll.Network.Http;

public class HttpCachedCall(Call call, HttpCache httpCache) : HttpCall(call)
{
	public HttpCache HttpCache = httpCache;

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
		{
			return Encoding.ASCII.GetString(bytes);
		}

		return null;
	}
}
