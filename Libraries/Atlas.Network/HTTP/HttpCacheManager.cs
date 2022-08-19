namespace Atlas.Network;

public class HttpCacheManager
{
	public HttpMemoryCache MemoryCache { get; set; } = new();

	private readonly Dictionary<string, HttpCache> _httpCaches = new();

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

	public void DeleteHttpCache(string path)
	{
		if (_httpCaches.TryGetValue(path, out HttpCache? httpCache))
		{
			httpCache.Dispose();
			_httpCaches.Remove(path);
		}

		if (Directory.Exists(path))
			Directory.Delete(path, true);
	}
}
