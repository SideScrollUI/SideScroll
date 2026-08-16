using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using SideScroll.Extensions;
using SideScroll.Utilities;

namespace SideScroll.Network.Http;

/// <summary>
/// HTTP response cache that stores deserialized objects with optional expiration
/// </summary>
public class HttpMemoryCache : IDisposable
{
	private readonly record struct CacheKey(string Key, Type Type);

	private bool _disposed;

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

	/// <summary>Gets or sets the JSON deserialization options used when parsing fetched responses.</summary>
	public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
	{
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
	};

	/// <summary>
	/// Initializes a new HTTP memory cache with specified size and duration limits
	/// </summary>
	public HttpMemoryCache(int? maxItems = null, TimeSpan? cacheDuration = null)
	{
		if (cacheDuration <= TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(cacheDuration), "Cache duration must be positive.");

		MaxItems = maxItems ?? DefaultMaxItems;
		CacheDuration = cacheDuration;

		// A SizeLimit of zero rejects every size one entry, so nothing would ever be cached
		// and every lookup would silently refetch
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxItems, nameof(maxItems));

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
	/// <remarks>
	/// Keyed by <typeparamref name="T"/> rather than the object's own type, so a uri stored through
	/// a base typed reference is found by <see cref="TryGetValue"/> asking for that same base type.
	/// The two only agree while both read the type from the call site
	/// </remarks>
	public void Add<T>(string key, T? obj)
	{
		if (obj == null)
			return;

		Add(key, obj, typeof(T));
	}

	private void Add(string key, object obj, Type type)
	{
		MemoryCacheEntryOptions options = new()
		{
			Size = 1, // Assume all items are the same size for now
		};

		if (CacheDuration.HasValue)
		{
			options.AbsoluteExpirationRelativeToNow = CacheDuration.Value;
		}

		MemoryCache.Set(new CacheKey(key, type), obj, options);
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

		if (MemoryCache.TryGetValue(new CacheKey(uri, typeof(T)), out T? result))
		{
			call.Log.Add("Found cached copy", new Tag("Uri", uri));
			t = result;
			return true;
		}

		try
		{
			string? text = HttpUtils.GetString(call, uri);
			if (text != null)
			{
				string escaped = JsonUtils.EscapeUnescapedControlCharactersInStrings(text);
				t = JsonSerializer.Deserialize<T>(escaped, JsonSerializerOptions);
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

	/// <summary>Releases the underlying memory cache.</summary>
	public void Dispose()
	{
		if (_disposed) return;

		_disposed = true;
		MemoryCache.Dispose();
		GC.SuppressFinalize(this);
	}
}
