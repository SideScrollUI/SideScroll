namespace SideScroll.Network.Http;

/// <summary>
/// Manages a set of open <see cref="HttpCache"/> instances keyed by directory path,
/// and provides an <see cref="HttpMemoryCache"/> for in-process caching.
/// </summary>
public class HttpCacheManager
{
	/// <summary>Gets or sets the in-process memory cache shared across all calls.</summary>
	public HttpMemoryCache MemoryCache { get; set; } = new();

	private readonly Dictionary<string, HttpCache> _httpCaches = [];

	/// <summary>Returns the open <see cref="HttpCache"/> for <paramref name="path"/>, opening it in read-write mode if not already open.</summary>
	// should we keep the imports open all the time?
	// should we be returning disposable references?
	public HttpCache OpenCache(string path)
	{
		if (_httpCaches.TryGetValue(path, out HttpCache? httpCache))
			return httpCache;

		httpCache = new HttpCache(path, true);
		_httpCaches[path] = httpCache;
		return httpCache;
	}

	/// <summary>Disposes and removes the cached <see cref="HttpCache"/> for <paramref name="path"/> and deletes its directory from disk.</summary>
	public void DeleteHttpCache(string path)
	{
		if (_httpCaches.TryGetValue(path, out HttpCache? httpCache))
		{
			httpCache.Dispose();
			_httpCaches.Remove(path);
		}

		if (Directory.Exists(path))
		{
			Directory.Delete(path, true);
		}
	}
}
