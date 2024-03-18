using Microsoft.Extensions.Caching.Memory;

namespace Atlas.Core.Collections;

[Unserialized]
public class MemoryTypeCache<T1>
{
	public int MaxItems { get; set; }
	public TimeSpan? CacheDuration { get; set; }

	public MemoryCache MemoryCache { get; init; }

	public MemoryTypeCache(int maxItems = 100, TimeSpan? cacheDuration = null)
	{
		MaxItems = maxItems;
		CacheDuration = cacheDuration;

		var options = new MemoryCacheOptions()
		{
			SizeLimit = MaxItems,
			ExpirationScanFrequency = TimeSpan.FromSeconds(60),
		};
		MemoryCache = new MemoryCache(options);
	}

	public void Set(string key, T1 value)
	{
		if (value == null) return;

		var options = new MemoryCacheEntryOptions()
		{
			Size = 1, // Assume all items are the same size for now
		};

		MemoryCache.Set(key, value, options);
	}

	public T1? Get(Call call, string key)
	{
		if (MemoryCache.TryGetValue(key, out object? obj))
		{
			call.Log.Add("Found cached copy", new Tag("Uri", key));
			return (T1)obj!;
		}

		return default;
	}

	public bool TryGetValue(string key, out T1? value)
	{
		if (MemoryCache.TryGetValue(key, out object? obj))
		{
			value = (T1)obj!;
			return true;
		}
		value = default;
		return false;
	}

	public bool TryGetValue(Call call, string key, out T1? value)
	{
		if (MemoryCache.TryGetValue(key, out object? obj))
		{
			call.Log.Add("Found cached copy", new Tag("Uri", key));
			value = (T1)obj!;
			return true;
		}
		value = default;
		return false;
	}
}
