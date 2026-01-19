using Microsoft.Extensions.Caching.Memory;
using SideScroll.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Collections;

/// <summary>
/// A typed memory cache wrapper that provides size-limited caching with optional expiration
/// </summary>
/// <typeparam name="T">The type of objects to cache</typeparam>
[Unserialized]
public class MemoryTypeCache<T>
{
	public int MaxItems { get; set; }
	public TimeSpan? CacheDuration { get; }

	public MemoryCache MemoryCache { get; }

	/// <summary>
	/// Initializes a new memory cache with specified size and duration limits
	/// </summary>
	public MemoryTypeCache(int maxItems = 100, TimeSpan? cacheDuration = null)
	{
		MaxItems = maxItems;
		CacheDuration = cacheDuration;

		MemoryCacheOptions options = new()
		{
			SizeLimit = MaxItems,
			ExpirationScanFrequency = TimeSpan.FromSeconds(60),
		};
		MemoryCache = new MemoryCache(options);
	}

	/// <summary>
	/// Stores a value in the cache with the specified key
	/// </summary>
	public void Set(string key, T value)
	{
		if (value == null) return;

		MemoryCacheEntryOptions options = new()
		{
			Size = 1, // Assume all items are the same size for now
		};

		MemoryCache.Set(key, value, options);
	}

	/// <summary>
	/// Retrieves a cached value and logs when found
	/// </summary>
	public T? Get(Call call, string key)
	{
		if (MemoryCache.TryGetValue(key, out object? obj))
		{
			call.Log.Add("Found cached copy", new Tag("Uri", key));
			return (T)obj!;
		}

		return default;
	}

	/// <summary>
	/// Attempts to retrieve a cached value
	/// </summary>
	public bool TryGetValue(string key, [NotNullWhen(true)] out T? value)
	{
		if (MemoryCache.TryGetValue(key, out object? obj))
		{
			value = (T)obj!;
			return true;
		}
		value = default;
		return false;
	}

	/// <summary>
	/// Attempts to retrieve a cached value and logs when found
	/// </summary>
	public bool TryGetValue(Call call, string key, [NotNullWhen(true)] out T? value)
	{
		if (MemoryCache.TryGetValue(key, out object? obj))
		{
			call.Log.Add("Found cached copy", new Tag("Uri", key));
			value = (T)obj!;
			return true;
		}
		value = default;
		return false;
	}
}
