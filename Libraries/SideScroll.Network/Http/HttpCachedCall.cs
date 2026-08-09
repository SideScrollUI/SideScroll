namespace SideScroll.Network.Http;

/// <summary>
/// An <see cref="HttpCall"/> that transparently serves responses from an <see cref="HttpCache"/>,
/// only hitting the network when the URI is not already cached.
/// </summary>
public class HttpCachedCall(Call call, HttpCache httpCache) : HttpCall(call)
{
	/// <summary>Gets the file-backed cache used to store and retrieve responses.</summary>
	public HttpCache HttpCache => httpCache;

	/// <summary>Returns the cached bytes for <paramref name="uri"/> if available, otherwise fetches from the network and stores the result.</summary>
	public override async Task<byte[]> GetBytesAsync(string uri, string? accept = null)
	{
		string key = GetCacheKey(uri, accept);

		byte[]? bytes = HttpCache.GetBytes(key);
		if (bytes != null)
			return bytes;

		bytes = await base.GetBytesAsync(uri, accept);
		HttpCache.AddEntry(key, bytes);
		return bytes;
	}

	/// <summary>Returns the cached or freshly fetched response for <paramref name="uri"/> decoded as text.</summary>
	public override async Task<string?> GetStringAsync(string uri, string? accept = null)
	{
		byte[] bytes = await GetBytesAsync(uri, accept);
		return HttpUtils.DecodeString(bytes);
	}

	// The same uri can return different content for each Accept header, so they can't share an entry.
	// Requests without one keep using the uri so existing caches still match
	private static string GetCacheKey(string uri, string? accept)
	{
		return accept == null ? uri : uri + '\n' + accept;
	}
}
