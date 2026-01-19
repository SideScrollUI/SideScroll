using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SideScroll.Extensions;

namespace SideScroll.Network.Http;

/// <summary>
/// HTTP response cache that stores deserialized objects with optional expiration
/// </summary>
public class HttpMemoryCache
{
	/// <summary>
	/// Gets or sets the default maximum number of items for new cache instances
	/// </summary>
	public static int DefaultMaxItems { get; set; } = 1000;

	/// <summary>
	/// Gets the maximum number of items this cache can hold
	/// </summary>
	public int MaxItems { get; }
	
	/// <summary>
	/// Gets the duration items remain in the cache before expiring
	/// </summary>
	public TimeSpan? CacheDuration { get; }

	/// <summary>
	/// Gets the underlying memory cache instance
	/// </summary>
	public MemoryCache MemoryCache { get; }

	/// <summary>
	/// Initializes a new HTTP memory cache with specified size and duration limits
	/// </summary>
	public HttpMemoryCache(int? maxItems = null, TimeSpan? cacheDuration = null)
	{
		MaxItems = maxItems ?? DefaultMaxItems;
		CacheDuration = cacheDuration;

		MemoryCacheOptions options = new()
		{
			SizeLimit = MaxItems,
		};

		if (CacheDuration.HasValue)
		{
			options.ExpirationScanFrequency = CacheDuration.Value.Min(TimeSpan.FromMinutes(1));
		}

		MemoryCache = new MemoryCache(options);
	}

	/// <summary>
	/// Adds an object to the cache with the specified key
	/// </summary>
	public void Add(string key, object? obj)
	{
		if (obj == null)
			return;

		MemoryCacheEntryOptions options = new()
		{
			Size = 1, // Assume all items are the same size for now
		};

		if (CacheDuration.HasValue)
		{
			options.AbsoluteExpirationRelativeToNow = CacheDuration.Value;
		}

		MemoryCache.Set(key, obj, options);
	}

	/// <summary>
	/// Retrieves a cached object or fetches it from the URI if not cached
	/// </summary>
	public T? Get<T>(Call call, string uri)
	{
		if (TryGetValue(call, uri, out T? t))
			return t;

		return default;
	}

	/// <summary>
	/// Attempts to retrieve a cached object or fetch and deserialize it from the URI
	/// </summary>
	public bool TryGetValue<T>(Call call, string uri, out T? t)
	{
		if (uri == null)
		{
			t = default;
			return false;
		}

		if (MemoryCache.TryGetValue(uri, out object? result))
		{
			call.Log.Add("Found cached copy", new Tag("Uri", uri));
			t = (T?)result;
			return true;
		}

		try
		{
			string? text = HttpUtils.GetString(call, uri);
			if (text != null)
			{
				// doesn't handle newlines
				//var options = new JsonSerializerOptions { IncludeFields = true };
				//t = JsonSerializer.Deserialize<T>(text, options);

				JsonSerializerSettings options = new()
				{
					DateTimeZoneHandling = DateTimeZoneHandling.Utc,
				};
				t = JsonConvert.DeserializeObject<T>(text, options);
				Add(uri, t);
				return true;
			}
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
		t = default;
		return false;
	}
}
