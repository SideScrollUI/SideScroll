using Microsoft.Extensions.Caching.Memory;
using SideScroll.Attributes;

namespace SideScroll.Collections;

[Unserialized]
public class MemoryTypeCache<T>
{
	public int MaxItems { get; set; }
	public TimeSpan? CacheDuration { get; set; }

	public MemoryCache MemoryCache { get; }

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

	public void Set(string key, T value)
	{
		if (value == null) return;

		MemoryCacheEntryOptions options = new()
		{
			Size = 1, // Assume all items are the same size for now
		};

		MemoryCache.Set(key, value, options);
	}

	public T? Get(Call call, string key)
	{
		if (MemoryCache.TryGetValue(key, out object? obj))
		{
			call.Log.Add("Found cached copy", new Tag("Uri", key));
			return (T)obj!;
		}

		return default;
	}

	public bool TryGetValue(string key, out T? value)
	{
		if (MemoryCache.TryGetValue(key, out object? obj))
		{
			value = (T)obj!;
			return true;
		}
		value = default;
		return false;
	}

	public bool TryGetValue(Call call, string key, out T? value)
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
